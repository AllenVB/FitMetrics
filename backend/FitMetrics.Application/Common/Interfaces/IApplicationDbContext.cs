using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Common.Interfaces;

/// <summary>
/// Application katmanının veritabanına EF Core'a doğrudan bağımlı olmadan eriştiği soyutlama.
/// Infrastructure'daki AppDbContext bunu implemente eder.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Food> Foods { get; }
    DbSet<NutritionLog> NutritionLogs { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<WorkoutLog> WorkoutLogs { get; }
    DbSet<WeightEntry> WeightEntries { get; }
    DbSet<DietitianClient> DietitianClients { get; }
    DbSet<KnowledgeEntry> KnowledgeEntries { get; }
    DbSet<WaterLog> WaterLogs { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<WorkoutPlan> WorkoutPlans { get; }
    DbSet<WorkoutPlanDay> WorkoutPlanDays { get; }
    DbSet<WorkoutPlanExercise> WorkoutPlanExercises { get; }
    DbSet<BodyMeasurement> BodyMeasurements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
