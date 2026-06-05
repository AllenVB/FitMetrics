using System.Text;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace FitMetrics.Infrastructure.Ai;

/// <summary>
/// Claude Messages API'ye HttpClient ile erişen istemci. Sistem promptu prompt-caching ile
/// işaretlenir; adaptive thinking, effort ve structured output (json_schema) desteklenir.
/// </summary>
public class ClaudeClient : IClaudeClient
{
    private readonly HttpClient _http;
    private readonly AnthropicSettings _settings;

    public ClaudeClient(HttpClient http, IOptions<AnthropicSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    public bool IsConfigured => _settings.IsConfigured;

    public Task<string> CompleteTextAsync(string systemPrompt, string userPrompt, ClaudeOptions options, CancellationToken ct = default)
        => SendAsync(BuildBody(systemPrompt, userPrompt, schema: null, options), ct);

    public Task<string> CompleteJsonAsync(string systemPrompt, string userPrompt, object jsonSchema, ClaudeOptions options, CancellationToken ct = default)
        => SendAsync(BuildBody(systemPrompt, userPrompt, jsonSchema, options), ct);

    public Task<string> AnalyzeImageJsonAsync(string systemPrompt, string userPrompt, string imageBase64, string mediaType, object jsonSchema, ClaudeOptions options, CancellationToken ct = default)
    {
        object content = new object[]
        {
            new { type = "image", source = new { type = "base64", media_type = mediaType, data = imageBase64 } },
            new { type = "text", text = userPrompt }
        };
        return SendAsync(BuildBody(systemPrompt, content, jsonSchema, options), ct);
    }

    public Task<string> ChatAsync(string systemPrompt, IReadOnlyList<ChatTurn> messages, ClaudeOptions options, CancellationToken ct = default)
    {
        var msgs = messages.Select(m => (object)new { role = m.Role, content = m.Content }).ToArray();
        return SendAsync(BuildBodyWithMessages(systemPrompt, msgs, schema: null, options), ct);
    }

    private Dictionary<string, object?> BuildBody(string system, object userContent, object? schema, ClaudeOptions options)
        => BuildBodyWithMessages(system, [new { role = "user", content = userContent }], schema, options);

    private Dictionary<string, object?> BuildBodyWithMessages(string system, object[] messages, object? schema, ClaudeOptions options)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = _settings.Model,
            ["max_tokens"] = options.MaxTokens,
            ["system"] = new object[]
            {
                new { type = "text", text = system, cache_control = new { type = "ephemeral" } }
            },
            ["messages"] = messages
        };

        if (options.Thinking)
            body["thinking"] = new { type = "adaptive" };

        var outputConfig = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(options.Effort))
            outputConfig["effort"] = options.Effort;
        if (schema is not null)
            outputConfig["format"] = new { type = "json_schema", schema };
        if (outputConfig.Count > 0)
            body["output_config"] = outputConfig;

        return body;
    }

    private async Task<string> SendAsync(Dictionary<string, object?> body, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new AiNotConfiguredException();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v1/messages");
        request.Headers.Add("x-api-key", _settings.ApiKey);
        request.Headers.Add("anthropic-version", _settings.AnthropicVersion);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException($"Claude API'ye ulaşılamadı: {ex.Message}");
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException($"Claude API hatası ({(int)response.StatusCode}).");

        return ExtractText(payload);
    }

    private static string ExtractText(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("stop_reason", out var stop) && stop.GetString() == "refusal")
            throw new ExternalServiceException("AI bu talebi güvenlik gerekçesiyle reddetti.");

        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var block in content.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var type) && type.GetString() == "text"
                    && block.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        throw new ExternalServiceException("AI yanıtı metin içermiyor.");
    }
}
