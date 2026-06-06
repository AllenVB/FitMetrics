using System.Text;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace FitMetrics.Infrastructure.Ai;

/// <summary>
/// Yerel Ollama LLM istemcisi (ücretsiz, anahtarsız). IClaudeClient'ı uygular; metin, JSON
/// (format=şema), ve görüntü (vision modeli + images) çağrılarını /api/chat üzerinden yapar.
/// </summary>
public class OllamaClient : IClaudeClient
{
    private readonly HttpClient _http;
    private readonly OllamaSettings _settings;

    public OllamaClient(HttpClient http, IOptions<OllamaSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    public bool IsConfigured => _settings.IsConfigured;

    public Task<string> CompleteTextAsync(string systemPrompt, string userPrompt, ClaudeOptions options, CancellationToken ct = default)
        => SendAsync(_settings.Model, new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        }, schema: null, options, ct);

    public Task<string> CompleteJsonAsync(string systemPrompt, string userPrompt, object jsonSchema, ClaudeOptions options, CancellationToken ct = default)
        => SendAsync(_settings.Model, new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        }, jsonSchema, options, ct);

    public Task<string> AnalyzeImageJsonAsync(string systemPrompt, string userPrompt, string imageBase64, string mediaType, object jsonSchema, ClaudeOptions options, CancellationToken ct = default)
        => SendAsync(_settings.VisionModel, new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt, images = new[] { imageBase64 } }
        }, jsonSchema, options, ct);

    public Task<string> ChatAsync(string systemPrompt, IReadOnlyList<ChatTurn> messages, ClaudeOptions options, CancellationToken ct = default)
    {
        var msgs = new List<object> { new { role = "system", content = systemPrompt } };
        msgs.AddRange(messages.Select(m => (object)new { role = m.Role, content = m.Content }));
        return SendAsync(_settings.Model, msgs.ToArray(), schema: null, options, ct);
    }

    private async Task<string> SendAsync(string model, object[] messages, object? schema, ClaudeOptions options, CancellationToken ct)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["stream"] = false,
            ["options"] = new { num_predict = options.MaxTokens, temperature = 0.6 }
        };
        if (schema is not null)
            body["format"] = schema;

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/chat";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new ExternalServiceException(
                $"Ollama'ya ulaşılamadı ({_settings.BaseUrl}). Ollama'nın çalıştığından ve '{model}' modelinin indirildiğinden emin olun.");
        }
        catch (TaskCanceledException)
        {
            throw new ExternalServiceException("Ollama yanıt vermedi (zaman aşımı). Daha küçük/hızlı bir model deneyin.");
        }

        var payload = await response.Content.ReadAsStringAsync(ct);

        JsonElement root;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            root = doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ExternalServiceException("Ollama yanıtı çözümlenemedi.");
        }

        if (root.TryGetProperty("error", out var error))
            throw new ExternalServiceException($"Ollama: {error.GetString()}");

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException($"Ollama hatası ({(int)response.StatusCode}).");

        if (root.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
            return content.GetString() ?? string.Empty;

        throw new ExternalServiceException("Ollama yanıtı beklenen biçimde değil.");
    }
}
