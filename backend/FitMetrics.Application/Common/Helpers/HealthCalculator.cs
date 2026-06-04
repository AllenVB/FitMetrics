using FitMetrics.Domain.Enums;

namespace FitMetrics.Application.Common.Helpers;

/// <summary>
/// Beslenme/sağlık hesaplamaları: BMR (Mifflin-St Jeor), TDEE, hedefe göre
/// günlük kalori ve protein hedefleri, BMI.
/// </summary>
public static class HealthCalculator
{
    /// <summary>Bazal metabolizma hızı (Mifflin-St Jeor).</summary>
    public static double CalculateBmr(Gender gender, double weightKg, double heightCm, int age)
    {
        var baseValue = (10 * weightKg) + (6.25 * heightCm) - (5 * age);
        return gender switch
        {
            Gender.Male => baseValue + 5,
            Gender.Female => baseValue - 161,
            _ => baseValue - 78 // erkek/kadın ortalaması
        };
    }

    public static double ActivityFactor(ActivityLevel level) => level switch
    {
        ActivityLevel.Sedentary => 1.2,
        ActivityLevel.Light => 1.375,
        ActivityLevel.Moderate => 1.55,
        ActivityLevel.Active => 1.725,
        ActivityLevel.VeryActive => 1.9,
        _ => 1.55
    };

    /// <summary>Toplam günlük enerji harcaması.</summary>
    public static double CalculateTdee(Gender gender, double weightKg, double heightCm, int age, ActivityLevel level)
        => CalculateBmr(gender, weightKg, heightCm, age) * ActivityFactor(level);

    /// <summary>Hedefe göre günlük kalori hedefi.</summary>
    public static int CalculateCalorieGoal(GoalType goal, double tdee) => goal switch
    {
        GoalType.LoseWeight => (int)Math.Round(tdee - 500),   // ~0.5 kg/hafta açık
        GoalType.GainMuscle => (int)Math.Round(tdee + 300),   // kontrollü kütle artışı
        _ => (int)Math.Round(tdee)
    };

    /// <summary>Hedefe göre günlük protein hedefi (gram).</summary>
    public static int CalculateProteinGoal(GoalType goal, double weightKg) => goal switch
    {
        GoalType.LoseWeight => (int)Math.Round(weightKg * 2.0),
        GoalType.GainMuscle => (int)Math.Round(weightKg * 2.0),
        _ => (int)Math.Round(weightKg * 1.6)
    };

    /// <summary>Vücut kitle indeksi.</summary>
    public static double CalculateBmi(double weightKg, double heightCm)
    {
        if (heightCm <= 0) return 0;
        var heightM = heightCm / 100.0;
        return Math.Round(weightKg / (heightM * heightM), 1);
    }
}
