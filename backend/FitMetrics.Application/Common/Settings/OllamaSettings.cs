namespace FitMetrics.Application.Common.Settings;

/// <summary>
/// Yerel Ollama LLM sunucusu yapılandırması (ücretsiz, anahtarsız).
/// AiProvider="Ollama" olduğunda Claude yerine bu kullanılır.
/// </summary>
public class OllamaSettings
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
    public string VisionModel { get; set; } = "llama3.2-vision";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Model);
}
