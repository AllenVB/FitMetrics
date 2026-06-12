using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

public class BodyMeasurement : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public double? WaistCm { get; set; }
    public double? HipCm { get; set; }
    public double? ChestCm { get; set; }
    public double? ArmCm { get; set; }
    public double? NeckCm { get; set; }
    public string? Notes { get; set; }
}
