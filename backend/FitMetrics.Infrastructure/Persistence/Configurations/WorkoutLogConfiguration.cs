using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.UserId, w.PerformedAt });

        builder.HasOne(w => w.Exercise)
            .WithMany(e => e.WorkoutLogs)
            .HasForeignKey(w => w.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
