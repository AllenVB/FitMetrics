using FitMetrics.Domain.Enums;

namespace FitMetrics.Application.DTOs.Nutrition;

public record FoodDto(
    int Id,
    string Name,
    string? Brand,
    string? Category,
    double CaloriesPer100g,
    double ProteinPer100g,
    double CarbsPer100g,
    double FatPer100g);

public record CreateFoodRequest(
    string Name,
    string? Brand,
    string? Category,
    double CaloriesPer100g,
    double ProteinPer100g,
    double CarbsPer100g,
    double FatPer100g);

public record CreateNutritionLogRequest(
    int FoodId,
    double AmountGrams,
    MealType MealType,
    DateTime? LoggedAt);

/// <summary>Barkod sorgusu sonucu (OpenFoodFacts). Henüz kaydedilmemiş, önizleme amaçlı.</summary>
public record BarcodeLookupResult(
    string Barcode,
    string Name,
    string? Brand,
    double CaloriesPer100g,
    double ProteinPer100g,
    double CarbsPer100g,
    double FatPer100g);

/// <summary>FatSecret besin arama sonucu öğesi (liste için).</summary>
public record FatSecretFoodResult(string Id, string Name, string? Brand, string Description);

/// <summary>Dış kaynaktan içe aktarılacak besinin 100g başına değerleri.</summary>
public record FoodImport(
    string Name,
    string? Brand,
    double CaloriesPer100g,
    double ProteinPer100g,
    double CarbsPer100g,
    double FatPer100g);

public record ImportFoodRequest(string FoodId);

public record NutritionLogDto(
    int Id,
    int FoodId,
    string FoodName,
    MealType MealType,
    double AmountGrams,
    double Calories,
    double Protein,
    double Carbs,
    double Fat,
    DateTime LoggedAt);

public record MealGroupDto(
    MealType MealType,
    double Calories,
    double Protein,
    double Carbs,
    double Fat,
    List<NutritionLogDto> Items);

public record DailyNutritionSummaryDto(
    DateTime Date,
    double TotalCalories,
    double TotalProtein,
    double TotalCarbs,
    double TotalFat,
    int CalorieGoal,
    int ProteinGoal,
    List<MealGroupDto> Meals);
