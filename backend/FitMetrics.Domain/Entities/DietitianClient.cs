using FitMetrics.Domain.Common;

namespace FitMetrics.Domain.Entities;

/// <summary>Bir diyetisyen ile danışanı (client) arasındaki bağ.</summary>
public class DietitianClient : BaseEntity
{
    public int DietitianId { get; set; }
    public User Dietitian { get; set; } = null!;

    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
}
