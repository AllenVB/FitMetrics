using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Infrastructure.Persistence;

/// <summary>
/// Referans verisi (besin ve egzersiz katalogu). HasData ile migration'a gömülür,
/// böylece veritabanı oluşturulduğunda hazır gelir.
/// </summary>
public static class SeedData
{
    private static readonly DateTime SeedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food>().HasData(GetFoods());
        modelBuilder.Entity<Exercise>().HasData(GetExercises());
    }

    private static Food[] GetFoods() =>
    [
        F(1,  "Tavuk Göğsü (Izgara)",      "Protein",       165, 31,   0,    3.6),
        F(2,  "Yumurta (Tam)",              "Protein",       155, 13,   1.1,  11),
        F(3,  "Yulaf Ezmesi",               "Tahıl",         389, 16.9, 66,   6.9),
        F(4,  "Beyaz Pirinç (Pişmiş)",      "Tahıl",         130, 2.7,  28,   0.3),
        F(5,  "Tam Buğday Ekmeği",          "Tahıl",         247, 13,   41,   3.4),
        F(6,  "Muz",                        "Meyve",         89,  1.1,  23,   0.3),
        F(7,  "Elma",                       "Meyve",         52,  0.3,  14,   0.2),
        F(8,  "Badem",                      "Kuruyemiş",     579, 21,   22,   50),
        F(9,  "Ceviz",                      "Kuruyemiş",     654, 15,   14,   65),
        F(10, "Süt (Yarım Yağlı)",          "Süt Ürünü",     50,  3.4,  5,    1.8),
        F(11, "Yoğurt (Tam Yağlı)",         "Süt Ürünü",     61,  3.5,  4.7,  3.3),
        F(12, "Lor Peyniri",                "Süt Ürünü",     98,  11,   3.4,  4.3),
        F(13, "Somon (Izgara)",             "Protein",       208, 20,   0,    13),
        F(14, "Ton Balığı (Suda)",          "Protein",       116, 26,   0,    1),
        F(15, "Dana Eti (Yağsız)",          "Protein",       250, 26,   0,    15),
        F(16, "Mercimek (Pişmiş)",          "Baklagil",      116, 9,    20,   0.4),
        F(17, "Nohut (Pişmiş)",             "Baklagil",      164, 8.9,  27,   2.6),
        F(18, "Brokoli",                    "Sebze",         34,  2.8,  7,    0.4),
        F(19, "Patates (Haşlanmış)",        "Sebze",         87,  1.9,  20,   0.1),
        F(20, "Tatlı Patates",              "Sebze",         86,  1.6,  20,   0.1),
        F(21, "Whey Protein Tozu",          "Takviye",       400, 80,   8,    6),
        F(22, "Zeytinyağı",                 "Yağ",           884, 0,    0,    100),
        F(23, "Avokado",                    "Meyve",         160, 2,    9,    15),
        F(24, "Bal",                        "Tatlandırıcı",  304, 0.3,  82,   0),
        F(25, "Makarna (Pişmiş)",           "Tahıl",         131, 5,    25,   1.1),
        F(26, "Tavuk But",                  "Protein",       209, 26,   0,    11),
        F(27, "Beyaz Peynir",               "Süt Ürünü",     264, 17,   2,    21),
        F(28, "Bulgur (Pişmiş)",            "Tahıl",         83,  3,    19,   0.2),
        F(29, "Hindi Göğsü",                "Protein",       135, 30,   0,    1),
        F(30, "Bitter Çikolata",            "Atıştırmalık",  546, 4.9,  61,   31)
    ];

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

    private static Food F(int id, string name, string category, double cal, double protein, double carbs, double fat) =>
        new()
        {
            Id = id,
            Name = name,
            Category = category,
            CaloriesPer100g = cal,
            ProteinPer100g = protein,
            CarbsPer100g = carbs,
            FatPer100g = fat,
            CreatedAt = SeedDate
        };

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
