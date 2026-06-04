using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Kilo / vücut yağı geçmişi kaydı (spec'teki "WeightHistory").
/// </summary>
public class WeightEntry : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public double WeightKg { get; set; }
    public double? BodyFatPercentage { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
