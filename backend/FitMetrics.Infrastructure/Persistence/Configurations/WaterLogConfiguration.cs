using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class WaterLogConfiguration : IEntityTypeConfiguration<WaterLog>
{
    public void Configure(EntityTypeBuilder<WaterLog> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.UserId, w.LoggedAt });
    }
}
