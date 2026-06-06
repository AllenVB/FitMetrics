using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.Common.Settings;
using FitMetrics.Infrastructure.Ai;
using FitMetrics.Infrastructure.External;
using FitMetrics.Infrastructure.Reports;
using FitMetrics.Infrastructure.Persistence;
using FitMetrics.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitMetrics.Infrastructure;

public static class DependencyInjection
{
    public const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=FitMetricsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? DefaultConnectionString;

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // AI sağlayıcısı: "Anthropic" (Claude, ücretli) veya "Ollama" (yerel, ücretsiz)
        services.Configure<AnthropicSettings>(configuration.GetSection(AnthropicSettings.SectionName));
        services.PostConfigure<AnthropicSettings>(s =>
        {
            if (string.IsNullOrWhiteSpace(s.ApiKey))
                s.ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? string.Empty;
        });
        services.Configure<OllamaSettings>(configuration.GetSection(OllamaSettings.SectionName));

        var aiProvider = configuration["AiProvider"] ?? "Anthropic";
        if (aiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            services.AddHttpClient<IClaudeClient, OllamaClient>(c => c.Timeout = TimeSpan.FromSeconds(300));
        else
            services.AddHttpClient<IClaudeClient, ClaudeClient>(c => c.Timeout = TimeSpan.FromSeconds(120));

        // PDF rapor üretimi (QuestPDF)
        services.AddSingleton<IPdfReportGenerator, QuestPdfReportGenerator>();

        // Barkod → besin (OpenFoodFacts, anahtarsız)
        services.AddHttpClient<IFoodLookupClient, OpenFoodFactsClient>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(15);
            c.DefaultRequestHeaders.Add("User-Agent", "FitMetrics/1.0 (educational project)");
        });

        // FatSecret besin arama (anahtarlar user-secrets / ortam değişkeniyle sağlanır)
        services.Configure<FatSecretSettings>(configuration.GetSection(FatSecretSettings.SectionName));
        services.AddHttpClient<IFatSecretClient, FatSecretClient>(c => c.Timeout = TimeSpan.FromSeconds(20));

        return services;
    }
}
