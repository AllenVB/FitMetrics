using FitMetrics.Application.DTOs.Nutrition;

namespace FitMetrics.Application.Common.Interfaces;

/// <summary>FatSecret Platform API portu (besin arama + detay). Implementasyon Infrastructure'dadır.</summary>
public interface IFatSecretClient
{
    bool IsConfigured { get; }
    Task<List<FatSecretFoodResult>> SearchAsync(string query, CancellationToken ct = default);
    Task<FoodImport?> GetFoodAsync(string foodId, CancellationToken ct = default);
}
