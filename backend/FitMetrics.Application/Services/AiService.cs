using System.Text;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Ai;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class AiService : IAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IClaudeClient _claude;
    private readonly IApplicationDbContext _db;
    private readonly IInsightService _insightService;

    public AiService(IClaudeClient claude, IApplicationDbContext db, IInsightService insightService)
    {
        _claude = claude;
        _db = db;
        _insightService = insightService;
    }

    public bool IsEnabled => _claude.IsConfigured;

    // ---- Akıllı öğün planı ----

    public async Task<MealPlanResponse> GenerateMealPlanAsync(int userId, GenerateMealPlanRequest request, CancellationToken ct = default)
    {
        EnsureEnabled();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var targetCalories = request.TargetCalories ?? user.DailyCalorieGoal;

        var system =
            "Sen FitMetrics uygulamasının uzman beslenme koçusun. Kullanıcının hedefine ve profiline uygun, " +
            "gerçekçi, dengeli ve uygulanabilir bir GÜNLÜK öğün planı oluştur. Türkçe yaz. Porsiyonları gram veya " +
            "adet olarak net belirt. Günlük toplam kaloriyi hedefe yakın tut. Yalnızca istenen JSON şemasına uygun yanıt ver.";

        var userPrompt = new StringBuilder()
            .AppendLine($"İstek: {request.Prompt}")
            .AppendLine($"Hedef tür: {GoalText(user.GoalType)}")
            .AppendLine($"Günlük kalori hedefi: ~{targetCalories} kcal")
            .AppendLine($"Günlük protein hedefi: ~{user.DailyProteinGoal} g")
            .AppendLine($"Vücut: {user.CurrentWeightKg} kg, {user.HeightCm} cm, {user.Age} yaş")
            .AppendLine("Kahvaltı, öğle, akşam ve ara öğün içerecek şekilde planla.")
            .ToString();

        var schema = MealPlanSchema();
        var json = await _claude.CompleteJsonAsync(system, userPrompt, schema, new ClaudeOptions(MaxTokens: 4000), ct);

        return Deserialize<MealPlanResponse>(json);
    }

    // ---- Doğal dil koçluk ----

    public async Task<CoachResponse> GenerateCoachingAsync(int userId, CancellationToken ct = default)
    {
        EnsureEnabled();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var insights = await _insightService.GenerateAsync(userId, ct);

        var analysis = new StringBuilder();
        foreach (var i in insights.Insights)
            analysis.AppendLine($"- [{i.Severity}] {i.Title}: {i.Message}{(i.Metric is null ? "" : $" ({i.Metric})")}");

        var system =
            "Sen FitMetrics'in kişisel sağlık ve performans koçusun. Aşağıdaki analizleri sıcak, motive edici ama " +
            "dürüst bir dille TEK bir bütünleşik koçluk mesajına dönüştür. Abartma, somut ve uygulanabilir öneriler ver. " +
            "Türkçe yaz. Ayrıca kullanıcının önümüzdeki hafta odaklanması gereken 2-4 maddeyi kısa ifadeler olarak çıkar. " +
            "Yalnızca istenen JSON şemasına uygun yanıt ver.";

        var userPrompt = new StringBuilder()
            .AppendLine($"Kullanıcı hedefi: {GoalText(user.GoalType)}")
            .AppendLine($"Profil: {user.CurrentWeightKg} kg, {user.HeightCm} cm, {user.Age} yaş, BMI {HealthCalculator.CalculateBmi(user.CurrentWeightKg, user.HeightCm)}")
            .AppendLine("Analizler:")
            .Append(analysis)
            .ToString();

        var schema = CoachSchema();
        var json = await _claude.CompleteJsonAsync(system, userPrompt, schema, new ClaudeOptions(MaxTokens: 1500), ct);

        return Deserialize<CoachResponse>(json);
    }

    // ---- Fotoğraftan yemek tanıma ----

    public async Task<MealPhotoResponse> AnalyzeMealPhotoAsync(AnalyzeMealPhotoRequest request, CancellationToken ct = default)
    {
        EnsureEnabled();

        var system =
            "Sen besin tanıma uzmanısın. Verilen yemek fotoğrafındaki yiyecekleri tanı ve makro/kalori tahmini yap. " +
            "Değerlerin tahmini olduğunu unutma; makul, gerçekçi sayılar ver. Türkçe yaz. " +
            "Yalnızca istenen JSON şemasına uygun yanıt ver.";

        const string userPrompt = "Bu fotoğraftaki yemeği analiz et; içindeki yiyecekleri ve tahmini kalori/makro değerlerini ver.";

        var schema = MealPhotoSchema();
        var json = await _claude.AnalyzeImageJsonAsync(
            system, userPrompt, request.ImageBase64, request.MediaType, schema,
            new ClaudeOptions(MaxTokens: 1500, Thinking: false), ct);

        return Deserialize<MealPhotoResponse>(json);
    }

    // ---- Yardımcılar ----

    private void EnsureEnabled()
    {
        if (!_claude.IsConfigured) throw new AiNotConfiguredException();
    }

    private static T Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                   ?? throw new ExternalServiceException("AI yanıtı boş döndü.");
        }
        catch (JsonException)
        {
            throw new ExternalServiceException("AI yanıtı beklenen biçimde değildi.");
        }
    }

    private static string GoalText(GoalType goal) => goal switch
    {
        GoalType.LoseWeight => "kilo verme",
        GoalType.GainMuscle => "kas kazanma",
        _ => "kilo koruma"
    };

    private static object MealPlanSchema() => new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "summary", "totalCalories", "totalProtein", "meals" },
        properties = new
        {
            summary = new { type = "string" },
            totalCalories = new { type = "integer" },
            totalProtein = new { type = "integer" },
            meals = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "mealType", "calories", "foods" },
                    properties = new
                    {
                        mealType = new { type = "string" },
                        calories = new { type = "integer" },
                        foods = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                required = new[] { "name", "amount", "calories", "protein" },
                                properties = new
                                {
                                    name = new { type = "string" },
                                    amount = new { type = "string" },
                                    calories = new { type = "integer" },
                                    protein = new { type = "integer" }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static object CoachSchema() => new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "message", "focusAreas" },
        properties = new
        {
            message = new { type = "string" },
            focusAreas = new { type = "array", items = new { type = "string" } }
        }
    };

    private static object MealPhotoSchema() => new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "description", "foods", "estimatedCalories", "estimatedProtein", "estimatedCarbs", "estimatedFat" },
        properties = new
        {
            description = new { type = "string" },
            foods = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "name", "portion", "calories", "protein" },
                    properties = new
                    {
                        name = new { type = "string" },
                        portion = new { type = "string" },
                        calories = new { type = "integer" },
                        protein = new { type = "integer" }
                    }
                }
            },
            estimatedCalories = new { type = "integer" },
            estimatedProtein = new { type = "integer" },
            estimatedCarbs = new { type = "integer" },
            estimatedFat = new { type = "integer" }
        }
    };
}
