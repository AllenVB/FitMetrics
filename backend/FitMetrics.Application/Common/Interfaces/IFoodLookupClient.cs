using FitMetrics.Application.DTOs.Nutrition;

namespace FitMetrics.Application.Common.Interfaces;

/// <summary>Barkoddan besin bilgisi çözen dış servis portu. Implementasyon (OpenFoodFacts) Infrastructure'dadır.</summary>
public interface IFoodLookupClient
{
    Task<BarcodeLookupResult?> LookupByBarcodeAsync(string barcode, CancellationToken ct = default);
}
