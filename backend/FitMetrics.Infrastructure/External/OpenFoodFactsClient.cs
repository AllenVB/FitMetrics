using System.Globalization;
using System.Text.Json;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Nutrition;

namespace FitMetrics.Infrastructure.External;

/// <summary>
/// Açık kaynak besin veritabanı OpenFoodFacts'ten barkoda göre ürün bilgisi çeker (anahtar gerektirmez).
/// </summary>
public class OpenFoodFactsClient : IFoodLookupClient
{
    private readonly HttpClient _http;

    public OpenFoodFactsClient(HttpClient http) => _http = http;

    public async Task<BarcodeLookupResult?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        var url = $"https://world.openfoodfacts.org/api/v2/product/{Uri.EscapeDataString(barcode)}.json" +
                  "?fields=product_name,brands,nutriments";

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(url, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        // status == 1 => ürün bulundu
        if (!root.TryGetProperty("status", out var status) || status.GetInt32() != 1)
            return null;
        if (!root.TryGetProperty("product", out var product))
            return null;

        var name = GetString(product, "product_name");
        if (string.IsNullOrWhiteSpace(name))
            name = $"Ürün {barcode}";
        var brand = GetString(product, "brands");

        double cal = 0, protein = 0, carbs = 0, fat = 0;
        if (product.TryGetProperty("nutriments", out var n))
        {
            cal = GetDouble(n, "energy-kcal_100g");
            protein = GetDouble(n, "proteins_100g");
            carbs = GetDouble(n, "carbohydrates_100g");
            fat = GetDouble(n, "fat_100g");
        }

        return new BarcodeLookupResult(
            barcode,
            name!,
            string.IsNullOrWhiteSpace(brand) ? null : brand,
            Math.Round(cal, 1),
            Math.Round(protein, 1),
            Math.Round(carbs, 1),
            Math.Round(fat, 1));
    }

    private static string? GetString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double GetDouble(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d)) return d;
        if (v.ValueKind == JsonValueKind.String &&
            double.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ds)) return ds;
        return 0;
    }
}
