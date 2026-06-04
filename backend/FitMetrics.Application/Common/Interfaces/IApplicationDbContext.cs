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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
