using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.Common.Settings;
using FitMetrics.Application.DTOs.Nutrition;
using Microsoft.Extensions.Options;

namespace FitMetrics.Infrastructure.External;

/// <summary>
/// FatSecret Platform API istemcisi. OAuth2 client_credentials token'ını süresiyle birlikte
/// önbelleğe alır; foods.search ve food.get.v4 (100g'a normalize) çağrılarını yapar.
/// </summary>
public class FatSecretClient : IFatSecretClient
{
    private const string TokenUrl = "https://oauth.fatsecret.com/connect/token";
    private const string ApiUrl = "https://platform.fatsecret.com/rest/server.api";

    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt;
    private static readonly SemaphoreSlim TokenLock = new(1, 1);

    private readonly HttpClient _http;
    private readonly FatSecretSettings _settings;

    public FatSecretClient(HttpClient http, IOptions<FatSecretSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    public bool IsConfigured => _settings.IsConfigured;

    public async Task<List<FatSecretFoodResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        var root = await CallAsync(
            $"{ApiUrl}?method=foods.search&format=json&max_results=20&search_expression={Uri.EscapeDataString(query)}", ct);

        var results = new List<FatSecretFoodResult>();
        if (root.TryGetProperty("foods", out var foods) && foods.TryGetProperty("food", out var food))
        {
            foreach (var f in AsArray(food))
            {
                var id = GetStr(f, "food_id");
                if (string.IsNullOrEmpty(id)) continue;
                results.Add(new FatSecretFoodResult(
                    id,
                    GetStr(f, "food_name") ?? "(isimsiz)",
                    GetStr(f, "brand_name"),
                    GetStr(f, "food_description") ?? string.Empty));
            }
        }
        return results;
    }

    public async Task<FoodImport?> GetFoodAsync(string foodId, CancellationToken ct = default)
    {
        var root = await CallAsync(
            $"{ApiUrl}?method=food.get.v4&format=json&food_id={Uri.EscapeDataString(foodId)}", ct);

        if (!root.TryGetProperty("food", out var food)) return null;

        var name = GetStr(food, "food_name") ?? "(isimsiz)";
        var brand = GetStr(food, "brand_name");

        if (!food.TryGetProperty("servings", out var servings) || !servings.TryGetProperty("serving", out var serving))
            return new FoodImport(name, brand, 0, 0, 0, 0);

        var list = AsArray(serving).ToList();

        // 100g'a normalize edilebilecek (gram/ml) bir porsiyon bul
        foreach (var s in list)
        {
            var unit = GetStr(s, "metric_serving_unit");
            var amount = GetNum(s, "metric_serving_amount");
            if ((unit == "g" || unit == "ml") && amount > 0)
            {
                var factor = 100.0 / amount;
                return new FoodImport(name, brand,
                    Round(GetNum(s, "calories") * factor),
                    Round(GetNum(s, "protein") * factor),
                    Round(GetNum(s, "carbohydrate") * factor),
                    Round(GetNum(s, "fat") * factor));
            }
        }

        // Gram bazlı porsiyon yoksa ilk porsiyonu olduğu gibi kullan (yaklaşık)
        var first = list.FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Object)
        {
            return new FoodImport(name, brand,
                Round(GetNum(first, "calories")),
                Round(GetNum(first, "protein")),
                Round(GetNum(first, "carbohydrate")),
                Round(GetNum(first, "fat")));
        }

        return new FoodImport(name, brand, 0, 0, 0, 0);
    }

    // ---- ortak çağrı + token ----

    private async Task<JsonElement> CallAsync(string url, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new ExternalServiceException("FatSecret API yapılandırılmamış (ClientId/ClientSecret eksik).");

        var token = await GetTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException($"FatSecret'e ulaşılamadı: {ex.Message}");
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out var error))
        {
            var msg = GetStr(error, "message") ?? "bilinmeyen hata";
            throw new ExternalServiceException($"FatSecret: {msg}");
        }

        return root.Clone();
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        await TokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = "basic"
            });

            var response = await _http.SendAsync(request, ct);
            var payload = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException("FatSecret token alınamadı (kimlik bilgilerini kontrol edin).");

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var token = GetStr(root, "access_token")
                        ?? throw new ExternalServiceException("FatSecret token yanıtı geçersiz.");
            var expiresIn = (int)GetNum(root, "expires_in");

            _cachedToken = token;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn > 60 ? expiresIn - 60 : expiresIn);
            return token;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    // ---- JSON yardımcıları (FatSecret sayıları string döndürür) ----

    private static IEnumerable<JsonElement> AsArray(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.Array => el.EnumerateArray(),
        JsonValueKind.Object => new[] { el },
        _ => []
    };

    private static string? GetStr(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double GetNum(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d)) return d;
        if (v.ValueKind == JsonValueKind.String &&
            double.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ds)) return ds;
        return 0;
    }

    private static double Round(double v) => Math.Round(v, 1);
}
