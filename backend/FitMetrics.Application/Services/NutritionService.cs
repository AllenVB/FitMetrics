using System.Text.Json;
using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Nutrition;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class NutritionService : INutritionService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IFoodLookupClient _foodLookup;
    private readonly IClaudeClient _claude;

    public NutritionService(IApplicationDbContext db, IMapper mapper, IFoodLookupClient foodLookup, IClaudeClient claude)
    {
        _db = db;
        _mapper = mapper;
        _foodLookup = foodLookup;
        _claude = claude;
    }

    public async Task<List<FoodDto>> GetFoodsAsync(string? search, CancellationToken ct = default)
    {
        var query = _db.Foods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(f => f.Name.Contains(term));
        }

        var foods = await query.OrderBy(f => f.Name).Take(100).ToListAsync(ct);
        return foods.Select(f => _mapper.Map<FoodDto>(f)).ToList();
    }

    public async Task<FoodDto> CreateFoodAsync(CreateFoodRequest request, CancellationToken ct = default)
    {
        var food = new Food
        {
            Name = request.Name.Trim(),
            Brand = request.Brand?.Trim(),
            Category = request.Category?.Trim(),
            CaloriesPer100g = request.CaloriesPer100g,
            ProteinPer100g = request.ProteinPer100g,
            CarbsPer100g = request.CarbsPer100g,
            FatPer100g = request.FatPer100g
        };

        _db.Foods.Add(food);
        await _db.SaveChangesAsync(ct);
        return _mapper.Map<FoodDto>(food);
    }

    public async Task<NutritionLogDto> AddLogAsync(int userId, CreateNutritionLogRequest request, CancellationToken ct = default)
    {
        var food = await _db.Foods.FirstOrDefaultAsync(f => f.Id == request.FoodId, ct)
                   ?? throw new NotFoundException("Besin", request.FoodId);

        var log = new NutritionLog
        {
            UserId = userId,
            FoodId = food.Id,
            AmountGrams = request.AmountGrams,
            MealType = request.MealType,
            LoggedAt = request.LoggedAt ?? DateTime.UtcNow
        };

        _db.NutritionLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        return ToDto(log, food);
    }

    public async Task DeleteLogAsync(int userId, int logId, CancellationToken ct = default)
    {
        var log = await _db.NutritionLogs.FirstOrDefaultAsync(n => n.Id == logId && n.UserId == userId, ct)
                  ?? throw new NotFoundException("Beslenme kaydı", logId);

        _db.NutritionLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<DailyNutritionSummaryDto> GetDailySummaryAsync(int userId, DateTime date, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var logs = await _db.NutritionLogs
            .Include(n => n.Food)
            .Where(n => n.UserId == userId && n.LoggedAt >= dayStart && n.LoggedAt < dayEnd)
            .ToListAsync(ct);

        var items = logs.Select(l => ToDto(l, l.Food)).ToList();

        var meals = Enum.GetValues<MealType>()
            .Select(mealType =>
            {
                var mealItems = items.Where(i => i.MealType == mealType).ToList();
                return new MealGroupDto(
                    mealType,
                    Math.Round(mealItems.Sum(i => i.Calories), 1),
                    Math.Round(mealItems.Sum(i => i.Protein), 1),
                    Math.Round(mealItems.Sum(i => i.Carbs), 1),
                    Math.Round(mealItems.Sum(i => i.Fat), 1),
                    mealItems);
            })
            .ToList();

        return new DailyNutritionSummaryDto(
            dayStart,
            Math.Round(items.Sum(i => i.Calories), 1),
            Math.Round(items.Sum(i => i.Protein), 1),
            Math.Round(items.Sum(i => i.Carbs), 1),
            Math.Round(items.Sum(i => i.Fat), 1),
            user.DailyCalorieGoal,
            user.DailyProteinGoal,
            meals);
    }

    public async Task<BarcodeLookupResult> LookupBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        var trimmed = barcode.Trim();
        var result = await _foodLookup.LookupByBarcodeAsync(trimmed, ct)
                     ?? throw new NotFoundException($"'{trimmed}' barkodlu ürün bulunamadı.");
        return result;
    }

    public async Task<List<AiFoodSuggestion>> SearchFoodsAsync(string query, CancellationToken ct = default)
    {
        var q = query.Trim();
        if (string.IsNullOrWhiteSpace(q)) return [];
        if (!_claude.IsConfigured)
            throw new ExternalServiceException("Besin araması için bir AI sağlayıcı (Ollama/Anthropic) gerekli.");

        const string system =
            "Sen bir beslenme veritabanısın. Kullanıcının aramasıyla eşleşen, BİRBİRİNDEN FARKLI ve GERÇEK yiyecekleri döndür. " +
            "Aynı yiyeceğin uydurma çeşitlerini/varyantlarını TEKRARLAMA; yalnızca yaygın, bilinen besinler ver. " +
            "Her sonuç için 100 GRAM başına gerçekçi ortalama değer ver: kalori (kcal), protein (g), karbonhidrat (g), yağ (g). " +
            "Değerler USDA/TÜİK benzeri ortalamalara dayansın; tahminî olduklarını unutma. Kısa Türkçe ad ve çok kısa açıklama yaz. " +
            "Marka yoksa boş bırak. En fazla 5 sonuç. Yalnızca istenen JSON şemasına uygun yanıt ver.";

        var userPrompt = $"Arama terimi: \"{q}\". Bu terime uyan en fazla 5 FARKLI gerçek yiyeceği 100g değerleriyle listele.";

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "results" },
            properties = new
            {
                results = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "name", "description", "caloriesPer100g", "proteinPer100g", "carbsPer100g", "fatPer100g" },
                        properties = new
                        {
                            name = new { type = "string" },
                            brand = new { type = "string" },
                            description = new { type = "string" },
                            caloriesPer100g = new { type = "number" },
                            proteinPer100g = new { type = "number" },
                            carbsPer100g = new { type = "number" },
                            fatPer100g = new { type = "number" }
                        }
                    }
                }
            }
        };

        var json = await _claude.CompleteJsonAsync(system, userPrompt, schema,
            new ClaudeOptions(MaxTokens: 1000, Thinking: false), ct);

        AiSearchWrapper? parsed;
        try { parsed = JsonSerializer.Deserialize<AiSearchWrapper>(json, JsonOpts); }
        catch (JsonException) { throw new ExternalServiceException("AI besin yanıtı çözümlenemedi."); }

        return (parsed?.Results ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new AiFoodSuggestion(
                r.Name.Trim(),
                string.IsNullOrWhiteSpace(r.Brand) ? null : r.Brand!.Trim(),
                string.IsNullOrWhiteSpace(r.Description) ? r.Name.Trim() : r.Description!.Trim(),
                Math.Max(0, Math.Round(r.CaloriesPer100g, 1)),
                Math.Max(0, Math.Round(r.ProteinPer100g, 1)),
                Math.Max(0, Math.Round(r.CarbsPer100g, 1)),
                Math.Max(0, Math.Round(r.FatPer100g, 1))))
            .Take(8)
            .ToList();
    }

    private sealed record AiSearchWrapper(List<AiSearchItem>? Results);
    private sealed record AiSearchItem(
        string Name, string? Brand, string? Description,
        double CaloriesPer100g, double ProteinPer100g, double CarbsPer100g, double FatPer100g);

    /// <summary>Bir öğün kaydını, besinin 100g değerlerini tüketilen gram miktarına ölçekleyerek DTO'ya çevirir.</summary>
    private static NutritionLogDto ToDto(NutritionLog log, Food food)
    {
        var factor = log.AmountGrams / 100.0;
        return new NutritionLogDto(
            log.Id,
            food.Id,
            food.Name,
            log.MealType,
            log.AmountGrams,
            Math.Round(food.CaloriesPer100g * factor, 1),
            Math.Round(food.ProteinPer100g * factor, 1),
            Math.Round(food.CarbsPer100g * factor, 1),
            Math.Round(food.FatPer100g * factor, 1),
            log.LoggedAt);
    }
}
