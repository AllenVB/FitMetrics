namespace FitMetrics.Application.Common.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "FitMetrics";
    public string Audience { get; set; } = "FitMetricsClient";
    public int ExpiryMinutes { get; set; } = 1440; // 24 saat
}
