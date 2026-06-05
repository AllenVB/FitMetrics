using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class DietitianClientConfiguration : IEntityTypeConfiguration<DietitianClient>
{
    public void Configure(EntityTypeBuilder<DietitianClient> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DietitianId, x.ClientId }).IsUnique();

        builder.HasOne(x => x.Dietitian)
            .WithMany()
            .HasForeignKey(x => x.DietitianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
