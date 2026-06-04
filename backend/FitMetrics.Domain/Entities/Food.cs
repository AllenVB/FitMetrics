using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Besin veritabanı kaydı. Tüm besin değerleri 100 gram başınadır;
/// gerçek tüketim <see cref="NutritionLog.AmountGrams"/> ile ölçeklenir.
/// </summary>
public class Food : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }

    public double CaloriesPer100g { get; set; }
    public double ProteinPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public double FatPer100g { get; set; }

    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();
}
