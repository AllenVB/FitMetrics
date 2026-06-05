using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Infrastructure.Persistence;

/// <summary>
/// Referans verisi (egzersiz katalogu). HasData ile migration'a gömülür.
/// Besinler FatSecret API üzerinden arama/içe aktarma ile sağlanır (statik seed yok).
/// </summary>
public static class SeedData
{
    private static readonly DateTime SeedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>().HasData(GetExercises());
    }

    private static Exercise[] GetExercises() =>
    [
        E(1,  "Bench Press",            ExerciseCategory.Strength, MuscleGroup.Chest,     6.0),
        E(2,  "Incline Dumbbell Press", ExerciseCategory.Strength, MuscleGroup.Chest,     6.0),
        E(3,  "Şınav (Push-up)",        ExerciseCategory.Strength, MuscleGroup.Chest,     7.0),
        E(4,  "Barfiks (Pull-up)",      ExerciseCategory.Strength, MuscleGroup.Back,      8.0),
        E(5,  "Lat Pulldown",           ExerciseCategory.Strength, MuscleGroup.Back,      6.0),
        E(6,  "Barbell Row",            ExerciseCategory.Strength, MuscleGroup.Back,      6.5),
        E(7,  "Deadlift",               ExerciseCategory.Strength, MuscleGroup.Back,      9.0),
        E(8,  "Squat",                  ExerciseCategory.Strength, MuscleGroup.Legs,      8.0),
        E(9,  "Leg Press",              ExerciseCategory.Strength, MuscleGroup.Legs,      7.0),
        E(10, "Lunge",                  ExerciseCategory.Strength, MuscleGroup.Legs,      7.0),
        E(11, "Leg Curl",               ExerciseCategory.Strength, MuscleGroup.Legs,      5.5),
        E(12, "Overhead Press",         ExerciseCategory.Strength, MuscleGroup.Shoulders, 6.0),
        E(13, "Lateral Raise",          ExerciseCategory.Strength, MuscleGroup.Shoulders, 4.5),
        E(14, "Biceps Curl",            ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(15, "Triceps Pushdown",       ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(16, "Plank",                  ExerciseCategory.Strength, MuscleGroup.Core,      4.0),
        E(17, "Mekik (Crunch)",         ExerciseCategory.Strength, MuscleGroup.Core,      4.5),
        E(18, "Koşu",                   ExerciseCategory.Cardio,   MuscleGroup.Cardio,    11.0),
        E(19, "Bisiklet",               ExerciseCategory.Cardio,   MuscleGroup.Cardio,    9.0),
        E(20, "Kürek (Rowing)",         ExerciseCategory.Cardio,   MuscleGroup.Cardio,    10.0),
        E(21, "İp Atlama",              ExerciseCategory.Cardio,   MuscleGroup.Cardio,    12.0),
        E(22, "Yürüyüş",                ExerciseCategory.Cardio,   MuscleGroup.Cardio,    5.0),
        E(23, "Yüzme",                  ExerciseCategory.Cardio,   MuscleGroup.Cardio,    10.0),
        E(24, "Eliptik",                ExerciseCategory.Cardio,   MuscleGroup.Cardio,    8.0)
    ];

    private static Exercise E(int id, string name, ExerciseCategory category, MuscleGroup muscle, double calPerMin) =>
        new()
        {
            Id = id,
            Name = name,
            Category = category,
            MuscleGroup = muscle,
            CaloriesBurnedPerMinute = calPerMin,
            CreatedAt = SeedDate
        };
}
