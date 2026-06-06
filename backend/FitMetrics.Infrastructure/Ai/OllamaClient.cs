using System.Runtime.CompilerServices;
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

    public async IAsyncEnumerable<string> ChatStreamAsync(string systemPrompt, IReadOnlyList<ChatTurn> messages, ClaudeOptions options, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var msgs = new List<object> { new { role = "system", content = systemPrompt } };
        msgs.AddRange(messages.Select(m => (object)new { role = m.Role, content = m.Content }));

        var body = new Dictionary<string, object?>
        {
            ["model"] = _settings.Model,
            ["messages"] = msgs.ToArray(),
            ["stream"] = true,
            ["keep_alive"] = "1h",
            ["options"] = new { num_predict = options.MaxTokens, temperature = 0.6 }
        };

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/api/chat";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (HttpRequestException)
        {
            request.Dispose();
            throw new ExternalServiceException($"Ollama'ya ulaşılamadı ({_settings.BaseUrl}). '{_settings.Model}' modelinin indirildiğinden emin olun.");
        }

        using (request)
        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException($"Ollama hatası ({(int)response.StatusCode}).");

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (true)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;
                if (line.Length == 0) continue;

                string? chunk = null;
                var done = false;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("message", out var m) && m.TryGetProperty("content", out var c))
                        chunk = c.GetString();
                    if (root.TryGetProperty("done", out var d) && d.ValueKind == JsonValueKind.True)
                        done = true;
                }
                catch (JsonException) { continue; }

                if (!string.IsNullOrEmpty(chunk)) yield return chunk;
                if (done) break;
            }
        }
    }

    private async Task<string> SendAsync(string model, object[] messages, object? schema, ClaudeOptions options, CancellationToken ct)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["stream"] = false,
            ["keep_alive"] = "1h", // modeli RAM'de tut → ilk yanıttaki yükleme gecikmesini önler
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
