using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Infrastructure.Persistence;

/// <summary>
/// Referans verisi (egzersiz katalogu). HasData ile migration'a gömülür.
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
        // ── Göğüs ────────────────────────────────────────────────────────────
        E(1,  "Bench Press (Barbell)",                 ExerciseCategory.Strength, MuscleGroup.Chest,     6.0),
        E(2,  "Incline Dumbbell Press",                ExerciseCategory.Strength, MuscleGroup.Chest,     6.0),
        E(3,  "Push-Up (Şınav)",                       ExerciseCategory.Strength, MuscleGroup.Chest,     7.0),
        E(25, "Chest Fly (Dumbbell / Cable)",          ExerciseCategory.Strength, MuscleGroup.Chest,     5.5),
        E(26, "Dips (Göğüs Odaklı)",                  ExerciseCategory.Strength, MuscleGroup.Chest,     7.0),
        E(27, "Decline Press (Ters Eğimli)",           ExerciseCategory.Strength, MuscleGroup.Chest,     6.0),

        // ── Sırt ─────────────────────────────────────────────────────────────
        E(7,  "Deadlift (Barbell)",                    ExerciseCategory.Strength, MuscleGroup.Back,      9.0),
        E(5,  "Lat Pulldown",                          ExerciseCategory.Strength, MuscleGroup.Back,      6.0),
        E(6,  "Bent Over Row (Barbell)",               ExerciseCategory.Strength, MuscleGroup.Back,      6.5),
        E(4,  "Pull-Up (Barfiks)",                     ExerciseCategory.Strength, MuscleGroup.Back,      8.0),
        E(28, "Seated Cable Row",                      ExerciseCategory.Strength, MuscleGroup.Back,      6.0),
        E(29, "T-Bar Row",                             ExerciseCategory.Strength, MuscleGroup.Back,      6.5),

        // ── Omuz ─────────────────────────────────────────────────────────────
        E(12, "Overhead Press (Military Press)",       ExerciseCategory.Strength, MuscleGroup.Shoulders, 6.0),
        E(13, "Dumbbell Lateral Raise",                ExerciseCategory.Strength, MuscleGroup.Shoulders, 4.5),
        E(30, "Dumbbell Shoulder Press",               ExerciseCategory.Strength, MuscleGroup.Shoulders, 5.5),
        E(31, "Front Raise (Dumbbell / Barbell)",      ExerciseCategory.Strength, MuscleGroup.Shoulders, 4.0),
        E(32, "Face Pull (Kablo)",                     ExerciseCategory.Strength, MuscleGroup.Shoulders, 4.5),
        E(33, "Reverse Fly (Dumbbell / Makine)",       ExerciseCategory.Strength, MuscleGroup.Shoulders, 4.0),

        // ── Bacak ─────────────────────────────────────────────────────────────
        E(8,  "Squat (Barbell)",                       ExerciseCategory.Strength, MuscleGroup.Legs,      8.0),
        E(9,  "Leg Press",                             ExerciseCategory.Strength, MuscleGroup.Legs,      7.0),
        E(34, "Romanian Deadlift (RDL)",               ExerciseCategory.Strength, MuscleGroup.Legs,      7.5),
        E(35, "Leg Extension",                         ExerciseCategory.Strength, MuscleGroup.Legs,      5.5),
        E(10, "Lunge (Dumbbell)",                      ExerciseCategory.Strength, MuscleGroup.Legs,      7.0),
        E(11, "Lying Leg Curl",                        ExerciseCategory.Strength, MuscleGroup.Legs,      5.5),

        // ── Kollar ───────────────────────────────────────────────────────────
        E(14, "Barbell Curl",                          ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(36, "Hammer Curl (Dumbbell)",                ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(37, "Preacher Curl",                         ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(15, "Triceps Pushdown (Kablo)",              ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),
        E(38, "Skull Crusher",                         ExerciseCategory.Strength, MuscleGroup.Arms,      4.5),
        E(39, "Overhead Triceps Extension (Dumbbell)", ExerciseCategory.Strength, MuscleGroup.Arms,      4.0),

        // ── Karın / Core ─────────────────────────────────────────────────────
        E(17, "Crunch (Mekik)",                        ExerciseCategory.Strength, MuscleGroup.Core,      4.5),
        E(16, "Plank",                                 ExerciseCategory.Strength, MuscleGroup.Core,      4.0),
        E(40, "Hanging Leg Raise",                     ExerciseCategory.Strength, MuscleGroup.Core,      5.0),
        E(41, "Russian Twist",                         ExerciseCategory.Strength, MuscleGroup.Core,      4.5),
        E(42, "Cable Crunch",                          ExerciseCategory.Strength, MuscleGroup.Core,      4.0),
        E(43, "Ab Wheel Rollout",                      ExerciseCategory.Strength, MuscleGroup.Core,      5.0),

        // ── Kardiyo ──────────────────────────────────────────────────────────
        E(18, "Koşu",                                  ExerciseCategory.Cardio,   MuscleGroup.Cardio,    11.0),
        E(19, "Bisiklet",                              ExerciseCategory.Cardio,   MuscleGroup.Cardio,    9.0),
        E(20, "Kürek (Rowing)",                        ExerciseCategory.Cardio,   MuscleGroup.Cardio,    10.0),
        E(21, "İp Atlama",                             ExerciseCategory.Cardio,   MuscleGroup.Cardio,    12.0),
        E(22, "Yürüyüş",                               ExerciseCategory.Cardio,   MuscleGroup.Cardio,    5.0),
        E(23, "Yüzme",                                 ExerciseCategory.Cardio,   MuscleGroup.Cardio,    10.0),
        E(24, "Eliptik",                               ExerciseCategory.Cardio,   MuscleGroup.Cardio,    8.0),
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
