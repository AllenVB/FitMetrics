namespace FitMetrics.Domain.Common;

/// <summary>
/// Tüm entity'ler için ortak temel sınıf. Kimlik ve denetim (audit) alanlarını taşır.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
