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
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public NutritionService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
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
