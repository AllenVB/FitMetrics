namespace FitMetrics.Application.Common.Interfaces;

/// <summary>Bir Claude isteği için ortak ayarlar.</summary>
public record ClaudeOptions(int MaxTokens = 2000, bool Thinking = true, string? Effort = "medium");

/// <summary>Çok turlu sohbette tek bir mesaj (role: "user" | "assistant").</summary>
public record ChatTurn(string Role, string Content);

/// <summary>
/// Claude Messages API'ye düşük seviyeli erişim portu. Implementasyon (HttpClient) Infrastructure'dadır.
/// Metin, JSON (structured output) ve görüntü (vision) tamamlamalarını destekler.
/// </summary>
public interface IClaudeClient
{
    /// <summary>API anahtarı tanımlı mı? Değilse AI özellikleri devre dışı bırakılmalıdır.</summary>
    bool IsConfigured { get; }

    /// <summary>Serbest metin yanıtı üretir.</summary>
    Task<string> CompleteTextAsync(string systemPrompt, string userPrompt, ClaudeOptions options, CancellationToken ct = default);

    /// <summary>Verilen JSON şemasına uygun yapılandırılmış JSON yanıtı üretir.</summary>
    Task<string> CompleteJsonAsync(string systemPrompt, string userPrompt, object jsonSchema, ClaudeOptions options, CancellationToken ct = default);

    /// <summary>Bir görüntüyü (base64) analiz edip JSON şemasına uygun yanıt üretir (vision).</summary>
    Task<string> AnalyzeImageJsonAsync(string systemPrompt, string userPrompt, string imageBase64, string mediaType, object jsonSchema, ClaudeOptions options, CancellationToken ct = default);

    /// <summary>Çok turlu sohbet: sistem promptu + mesaj geçmişiyle metin yanıtı üretir.</summary>
    Task<string> ChatAsync(string systemPrompt, IReadOnlyList<ChatTurn> messages, ClaudeOptions options, CancellationToken ct = default);

    /// <summary>Sohbet yanıtını parça parça (streaming) döndürür.</summary>
    IAsyncEnumerable<string> ChatStreamAsync(string systemPrompt, IReadOnlyList<ChatTurn> messages, ClaudeOptions options, CancellationToken ct = default);
}
