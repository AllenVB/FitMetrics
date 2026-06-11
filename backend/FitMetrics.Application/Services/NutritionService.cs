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
    private readonly IFatSecretClient _fatSecret;

    public NutritionService(IApplicationDbContext db, IMapper mapper, IFoodLookupClient foodLookup,
        IClaudeClient claude, IFatSecretClient fatSecret)
    {
        _db       = db;
        _mapper   = mapper;
        _foodLookup = foodLookup;
        _claude   = claude;
        _fatSecret = fatSecret;
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

        // ── 1. FatSecret (gerçek veritabanı, öncelikli) ──────────────────────
        if (_fatSecret.IsConfigured)
        {
            using var fsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            fsCts.CancelAfter(TimeSpan.FromSeconds(5)); // IP whitelist onaylanmamışsa uzun bekleme
            try
            {
                var fsResults = await _fatSecret.SearchAsync(q, fsCts.Token);
                var suggestions = fsResults
                    .Select(r => ParseFatSecretDescription(r.Name, r.Brand, r.Description))
                    .Where(s => s is not null && s.CaloriesPer100g > 0)
                    .Take(8)
                    .ToList();

                if (suggestions.Count > 0)
                    return suggestions!;
            }
            catch
            {
                // FatSecret başarısız veya timeout → AI'a düş
            }
        }

        // ── 2. AI yedek (Ollama / Anthropic) ─────────────────────────────────
        if (!_claude.IsConfigured)
            throw new ExternalServiceException(
                "Besin araması için FatSecret veya bir AI sağlayıcı (Ollama/Anthropic) gerekli.");

        // Constrained JSON decoding (CompleteJsonAsync) yerine free-text kullan;
        // Ollama ile ~5s vs ~70s. JSON'ı metin içinden extract et.
        // Sadece 3 sonuç, kısa description → az token → hızlı
        const string system = "Beslenme DB. Sadece JSON array döndür:\n" +
            "[{\"name\":\"Ad\",\"brand\":null,\"description\":\"Kısa açıklama\",\"caloriesPer100g\":0,\"proteinPer100g\":0,\"carbsPer100g\":0,\"fatPer100g\":0}]";

        var userPrompt = $"'{q}' için 3 farklı besin, 100g USDA, sadece JSON array:";

        var rawText = await _claude.CompleteTextAsync(system, userPrompt,
            new ClaudeOptions(MaxTokens: 250, Thinking: false), ct);

        // JSON array'ı metin içinden çıkar
        var json = ExtractJsonArray(rawText);

        List<AiSearchItem>? parsed;
        try { parsed = JsonSerializer.Deserialize<List<AiSearchItem>>(json, JsonOpts); }
        catch (JsonException) { throw new ExternalServiceException("AI besin yanıtı çözümlenemedi."); }

        return (parsed ?? [])
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

    /// <summary>
    /// FatSecret'in "Per 100g - Calories: 165kcal | Fat: 3.57g | Carbs: 0.00g | Protein: 31.02g"
    /// formatındaki description'ını parse eder. Farklı porsiyon boyutlarını 100g'a normalize eder.
    /// </summary>
    private static AiFoodSuggestion? ParseFatSecretDescription(string name, string? brand, string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;

        // Porsiyon miktarını bul: "Per 100g", "Per 1 serving (85g)", "Per 1 cup (240ml)" vb.
        double normFactor = 1.0;
        var perMatch = System.Text.RegularExpressions.Regex.Match(
            description, @"Per\s+[\d.,]+\s+\w+\s+\(([\d.,]+)\s*[gm]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (perMatch.Success && double.TryParse(perMatch.Groups[1].Value,
                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var servingG)
            && servingG > 0)
        {
            normFactor = 100.0 / servingG;
        }

        double cal     = ParseNutrient(description, @"Calories:\s*([\d.,]+)");
        double fat     = ParseNutrient(description, @"Fat:\s*([\d.,]+)");
        double carbs   = ParseNutrient(description, @"Carbs:\s*([\d.,]+)");
        double protein = ParseNutrient(description, @"Protein:\s*([\d.,]+)");

        if (cal <= 0) return null;

        return new AiFoodSuggestion(
            name.Trim(),
            string.IsNullOrWhiteSpace(brand) ? null : brand.Trim(),
            description.Length > 80 ? description[..80] : description,
            Math.Round(cal     * normFactor, 1),
            Math.Round(protein * normFactor, 1),
            Math.Round(carbs   * normFactor, 1),
            Math.Round(fat     * normFactor, 1));
    }

    private static double ParseNutrient(string text, string pattern)
    {
        var m = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return 0;
        return double.TryParse(m.Groups[1].Value,
            System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    /// <summary>Metin içindeki ilk JSON array'ı ([ ... ]) bulur; yoksa boş array döner.</summary>
    private static string ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end   = text.LastIndexOf(']');
        return start >= 0 && end > start ? text[start..(end + 1)] : "[]";
    }

    private sealed class AiSearchItem
    {
        public string Name { get; set; } = "";
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public double CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double CarbsPer100g { get; set; }
        public double FatPer100g { get; set; }
    }

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
