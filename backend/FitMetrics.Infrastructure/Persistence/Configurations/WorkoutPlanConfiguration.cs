using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitMetrics.Infrastructure.Persistence.Configurations;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.UserId);

        builder.HasMany(p => p.Days)
            .WithOne(d => d.WorkoutPlan)
            .HasForeignKey(d => d.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkoutPlanDayConfiguration : IEntityTypeConfiguration<WorkoutPlanDay>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanDay> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.WorkoutPlanId, d.DayIndex }).IsUnique();

        builder.HasMany(d => d.Exercises)
            .WithOne(e => e.Day)
            .HasForeignKey(e => e.WorkoutPlanDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Exercise)
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
