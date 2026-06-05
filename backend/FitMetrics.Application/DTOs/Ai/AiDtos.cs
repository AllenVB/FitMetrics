namespace FitMetrics.Application.DTOs.Ai;

// ---- Akıllı öğün / program oluşturucu ----

public record GenerateMealPlanRequest(string Prompt, int? TargetCalories);

public record MealPlanFood(string Name, string Amount, int Calories, int Protein);

public record MealPlanMeal(string MealType, int Calories, List<MealPlanFood> Foods);

public record MealPlanResponse(
    string Summary,
    int TotalCalories,
    int TotalProtein,
    List<MealPlanMeal> Meals);

// ---- Doğal dil koçluk ----

public record CoachResponse(string Message, List<string> FocusAreas);

// ---- Fotoğraftan yemek tanıma (vision) ----

public record AnalyzeMealPhotoRequest(string ImageBase64, string MediaType);

public record DetectedFood(string Name, string Portion, int Calories, int Protein);

public record MealPhotoResponse(
    string Description,
    List<DetectedFood> Foods,
    int EstimatedCalories,
    int EstimatedProtein,
    int EstimatedCarbs,
    int EstimatedFat);
