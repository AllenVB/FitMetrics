namespace FitMetrics.Application.Common.Settings;

/// <summary>
/// FatSecret Platform API yapılandırması. Anahtarlar boşsa besin arama zarifçe devre dışı kalır.
/// Gizli anahtar repo'ya yazılmaz; user-secrets veya ortam değişkeni (FatSecret__ClientSecret) ile sağlanır.
/// </summary>
public class FatSecretSettings
{
    public const string SectionName = "FatSecret";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
