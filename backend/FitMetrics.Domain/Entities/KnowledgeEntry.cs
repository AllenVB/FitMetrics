using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>
/// Yöneticinin tanımladığı onaylı soru/cevap. Sohbet asistanı, ilgili soruda bu içeriği
/// temel alarak yanıt verir (bağlam/grounding — fine-tuning değil).
/// </summary>
public class KnowledgeEntry : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
