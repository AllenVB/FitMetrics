namespace FitMetrics.Application.Common.Settings;

/// <summary>
/// Claude API (Anthropic) yapılandırması. ApiKey boşsa AI özellikleri zarifçe devre dışı kalır.
/// </summary>
public class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-opus-4-8";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string AnthropicVersion { get; set; } = "2023-06-01";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
