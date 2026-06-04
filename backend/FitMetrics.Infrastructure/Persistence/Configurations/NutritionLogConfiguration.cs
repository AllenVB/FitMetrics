using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class NutritionLogConfiguration : IEntityTypeConfiguration<NutritionLog>
{
    public void Configure(EntityTypeBuilder<NutritionLog> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.MealType).HasConversion<int>();
        builder.HasIndex(n => new { n.UserId, n.LoggedAt });

        builder.HasOne(n => n.Food)
            .WithMany(f => f.NutritionLogs)
            .HasForeignKey(n => n.FoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
