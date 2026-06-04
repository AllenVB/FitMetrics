using FitMetrics.Domain.Common;
using FitMetrics.Domain.Enums;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Kullanıcının tek bir öğün girişi (spec'teki "DailyNutrition" tablosuna karşılık gelir).
/// </summary>
public class NutritionLog : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int FoodId { get; set; }
    public Food Food { get; set; } = null!;

    public double AmountGrams { get; set; }
    public MealType MealType { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
