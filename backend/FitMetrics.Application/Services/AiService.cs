using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Ai;
using FitMetrics.Application.DTOs.Dashboard;
using FitMetrics.Application.DTOs.Insights;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class AiService : IAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IClaudeClient _claude;
    private readonly IApplicationDbContext _db;
    private readonly IInsightService _insightService;
    private readonly IDashboardService _dashboardService;

    public AiService(IClaudeClient claude, IApplicationDbContext db, IInsightService insightService, IDashboardService dashboardService)
    {
        _claude = claude;
        _db = db;
        _insightService = insightService;
        _dashboardService = dashboardService;
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

    // ---- Sohbet asistanı ----

    public async Task<ChatResponse> ChatAsync(int userId, ChatRequest request, CancellationToken ct = default)
    {
        EnsureEnabled();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var dashboard = await _dashboardService.GetDashboardAsync(userId, ct);
        var insights = await _insightService.GenerateAsync(userId, ct);

        var system = BuildChatSystemPrompt(user, dashboard, insights);

        // Son 20 mesaj; sohbet kullanıcı mesajıyla başlamalı
        var turns = request.Messages
            .TakeLast(20)
            .Select(m => new ChatTurn(m.Role, m.Content))
            .ToList();
        while (turns.Count > 0 && turns[0].Role != "user")
            turns.RemoveAt(0);
        if (turns.Count == 0)
            throw new ExternalServiceException("Geçerli bir kullanıcı mesajı bulunamadı.");

        // CPU inference için kısa tutuyoruz → daha hızlı yanıt
        var reply = await _claude.ChatAsync(system, turns,
            new ClaudeOptions(MaxTokens: 600, Thinking: false, Effort: "medium"), ct);

        return new ChatResponse(reply.Trim());
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(int userId, ChatRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        EnsureEnabled();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var dashboard = await _dashboardService.GetDashboardAsync(userId, ct);
        var insights = await _insightService.GenerateAsync(userId, ct);
        var system = BuildChatSystemPrompt(user, dashboard, insights);

        var turns = request.Messages
            .TakeLast(20)
            .Select(m => new ChatTurn(m.Role, m.Content))
            .ToList();
        while (turns.Count > 0 && turns[0].Role != "user")
            turns.RemoveAt(0);
        if (turns.Count == 0)
            throw new ExternalServiceException("Geçerli bir kullanıcı mesajı bulunamadı.");

        await foreach (var chunk in _claude.ChatStreamAsync(system, turns,
                           new ClaudeOptions(MaxTokens: 600, Thinking: false, Effort: "medium"), ct))
            yield return chunk;
    }

    private static string BuildChatSystemPrompt(User user, DashboardDto d, InsightsResponse insights)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sen FitMetrics uygulamasının kişisel sağlık ve fitness asistanısın.");
        sb.AppendLine("Kullanıcının aşağıdaki GERÇEK verisine dayanarak Türkçe; kısa, somut ve motive edici yanıtlar ver.");
        sb.AppendLine("Tıbbi teşhis/tedavi verme; genel sağlık-fitness rehberliği sun. Veride olmayanı uydurma; eksikse kullanıcıyı veri girmeye yönlendir.");
        sb.AppendLine();
        sb.AppendLine("[KULLANICI VERİSİ]");
        sb.AppendLine($"Ad: {user.FullName} · {user.Age} yaş · {GenderText(user.Gender)} · {user.HeightCm} cm · {user.CurrentWeightKg} kg · BMI {d.Bmi}");
        sb.AppendLine($"Hedef: {GoalText(user.GoalType)}{(user.TargetWeightKg.HasValue ? $" · hedef kilo {user.TargetWeightKg} kg" : "")}");
        sb.AppendLine($"Günlük hedef: {d.Today.CalorieGoal} kcal · {d.Today.ProteinGoal} g protein · {d.WaterGoalMl} ml su");
        sb.AppendLine($"Bugün alınan: {Math.Round(d.Today.TotalCalories)} kcal · {Math.Round(d.Today.TotalProtein)} g protein · {Math.Round(d.Today.TotalCarbs)} g karb · {Math.Round(d.Today.TotalFat)} g yağ");
        sb.AppendLine($"Bu hafta: {d.WorkoutsThisWeek} antrenman · {Math.Round(d.CaloriesBurnedThisWeek)} kcal yakıldı");
        sb.AppendLine();
        sb.AppendLine("[GÜNCEL ANALİZLER]");
        foreach (var i in insights.Insights)
            sb.AppendLine($"- {i.Title}: {i.Message}");
        return sb.ToString();
    }

    private static string GenderText(Gender gender) => gender switch
    {
        Gender.Male => "erkek",
        Gender.Female => "kadın",
        _ => "diğer"
    };

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
