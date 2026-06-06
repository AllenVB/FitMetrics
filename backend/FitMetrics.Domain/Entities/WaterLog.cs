using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Tek bir su tüketim kaydı (ml). Günlük toplam, gün bazında bu kayıtların toplanmasıyla bulunur.
/// </summary>
public class WaterLog : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int AmountMl { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
