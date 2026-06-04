using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Insights;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using FitMetrics.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

/// <summary>
/// Kural tabanlı analiz motoru. Kullanıcının son haftalardaki beslenme, antrenman ve
/// kilo verilerini inceleyerek kişiselleştirilmiş, sayısal temelli öneriler üretir.
/// </summary>
public class InsightService : IInsightService
{
    private const double CaloriesPerKgFat = 7700;
    private const int NutritionWindowDays = 14;
    private const int WorkoutWindowDays = 21;

    private readonly IApplicationDbContext _db;

    public InsightService(IApplicationDbContext db) => _db = db;

    public async Task<InsightsResponse> GenerateAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        var now = DateTime.UtcNow;
        var nutritionStart = now.Date.AddDays(-(NutritionWindowDays - 1));
        var workoutStart = now.Date.AddDays(-(WorkoutWindowDays - 1));

        var nutritionLogs = await _db.NutritionLogs
            .Include(n => n.Food)
            .Where(n => n.UserId == userId && n.LoggedAt >= nutritionStart)
            .ToListAsync(ct);

        var workoutLogs = await _db.WorkoutLogs
            .Include(w => w.Exercise)
            .Where(w => w.UserId == userId && w.PerformedAt >= workoutStart)
            .ToListAsync(ct);

        var weightEntries = await _db.WeightEntries.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.RecordedAt)
            .ToListAsync(ct);

        var insights = new List<InsightDto>();

        AddCalorieInsight(insights, user, nutritionLogs);
        AddProteinInsight(insights, user, nutritionLogs);
        AddMacroInsight(insights, nutritionLogs);
        AddWorkoutInsight(insights, workoutLogs);
        AddWeightInsight(insights, user, weightEntries);
        AddConsistencyInsight(insights, nutritionLogs);

        if (insights.Count == 0)
        {
            insights.Add(new InsightDto(
                InsightCategory.Consistency, InsightSeverity.Info,
                "Veri Toplanıyor",
                "Analiz için henüz yeterli veri yok. Birkaç gün beslenme ve antrenman kaydı girdikçe kişiselleştirilmiş öneriler burada görünecek.",
                null));
        }

        return new InsightsResponse(now, NutritionWindowDays, insights);
    }

    // ---- Beslenme toplamı ----

    private record NutritionAggregate(int LoggedDays, double TotalCalories, double TotalProtein, double TotalCarbs, double TotalFat)
    {
        public double AvgCalories => LoggedDays > 0 ? TotalCalories / LoggedDays : 0;
        public double AvgProtein => LoggedDays > 0 ? TotalProtein / LoggedDays : 0;
    }

    private static NutritionAggregate Aggregate(List<NutritionLog> logs)
    {
        double cal = 0, protein = 0, carbs = 0, fat = 0;
        foreach (var l in logs)
        {
            var factor = l.AmountGrams / 100.0;
            cal += l.Food.CaloriesPer100g * factor;
            protein += l.Food.ProteinPer100g * factor;
            carbs += l.Food.CarbsPer100g * factor;
            fat += l.Food.FatPer100g * factor;
        }

        var loggedDays = logs.Select(l => l.LoggedAt.Date).Distinct().Count();
        return new NutritionAggregate(loggedDays, cal, protein, carbs, fat);
    }

    // ---- Analizler ----

    private static void AddCalorieInsight(List<InsightDto> list, User user, List<NutritionLog> logs)
    {
        var agg = Aggregate(logs);
        if (agg.LoggedDays < 3) return;

        var tdee = HealthCalculator.CalculateTdee(user.Gender, user.CurrentWeightKg, user.HeightCm, user.Age, user.ActivityLevel);
        var deficit = tdee - agg.AvgCalories; // pozitif => açık
        var monthlyKg = Math.Round(Math.Abs(deficit) * 30 / CaloriesPerKgFat, 1);
        var metric = $"Ø {Math.Round(agg.AvgCalories)} kcal/gün · Bakım {Math.Round(tdee)} kcal";

        if (deficit > 100)
        {
            var positive = user.GoalType != GoalType.GainMuscle;
            var msg = positive
                ? $"Son {agg.LoggedDays} günde günlük ortalama {Math.Round(deficit)} kalori açık oluşturuyorsun. Bu hızla devam edersen ayda yaklaşık {monthlyKg} kg yağ kaybı beklenebilir."
                : $"Son {agg.LoggedDays} günde günlük ortalama {Math.Round(deficit)} kalori açıktasın. Kas kazanım hedefin için bu açığı kapatıp hafif bir kalori fazlasına geçmen önerilir.";
            list.Add(new InsightDto(InsightCategory.Calories, positive ? InsightSeverity.Positive : InsightSeverity.Warning, "Kalori Dengesi", msg, metric));
        }
        else if (deficit < -100)
        {
            var surplus = Math.Round(-deficit);
            var warn = user.GoalType == GoalType.LoseWeight;
            var msg = warn
                ? $"Son {agg.LoggedDays} günde günlük ortalama {surplus} kalori fazla alıyorsun. Kilo verme hedefin için porsiyonları gözden geçir; bu fazla ayda ~{monthlyKg} kg artışa yol açabilir."
                : $"Son {agg.LoggedDays} günde günlük ortalama {surplus} kalori fazlası alıyorsun. Kas kazanımı için kontrollü bir fazla uygun olabilir.";
            list.Add(new InsightDto(InsightCategory.Calories, warn ? InsightSeverity.Warning : InsightSeverity.Positive, "Kalori Dengesi", msg, metric));
        }
        else
        {
            list.Add(new InsightDto(InsightCategory.Calories, InsightSeverity.Info, "Kalori Dengesi",
                $"Kalori alımın bakım seviyene çok yakın (Ø {Math.Round(agg.AvgCalories)} kcal). Kilonu korumak için ideal seviyedesin.", metric));
        }
    }

    private static void AddProteinInsight(List<InsightDto> list, User user, List<NutritionLog> logs)
    {
        var agg = Aggregate(logs);
        if (agg.LoggedDays < 3 || user.DailyProteinGoal <= 0) return;

        var avg = agg.AvgProtein;
        var goal = user.DailyProteinGoal;
        var metric = $"Ø {Math.Round(avg)} g/gün · Hedef {goal} g";

        if (avg < goal * 0.9)
        {
            var pct = (int)Math.Round((goal - avg) / goal * 100);
            list.Add(new InsightDto(InsightCategory.Protein, InsightSeverity.Warning, "Protein Alımı",
                $"Günlük protein alımın hedefinin %{pct} altında. Kas gelişimini desteklemek için günlük {goal} grama ulaşmayı hedefle (tavuk göğsü, yumurta, süzme peynir, whey).", metric));
        }
        else
        {
            list.Add(new InsightDto(InsightCategory.Protein, InsightSeverity.Positive, "Protein Alımı",
                $"Protein hedefini tutturuyorsun (Ø {Math.Round(avg)} g). Kas koruması ve gelişimi için harika gidiyor.", metric));
        }
    }

    private static void AddMacroInsight(List<InsightDto> list, List<NutritionLog> logs)
    {
        var agg = Aggregate(logs);
        if (agg.LoggedDays < 3) return;

        var pCal = agg.TotalProtein * 4;
        var cCal = agg.TotalCarbs * 4;
        var fCal = agg.TotalFat * 9;
        var sum = pCal + cCal + fCal;
        if (sum <= 0) return;

        var p = (int)Math.Round(pCal / sum * 100);
        var c = (int)Math.Round(cCal / sum * 100);
        var f = (int)Math.Round(fCal / sum * 100);

        list.Add(new InsightDto(InsightCategory.Macros, InsightSeverity.Info, "Makro Dağılımı",
            $"Kalorinin yaklaşık %{p}'i proteinden, %{c}'i karbonhidrattan, %{f}'i yağdan geliyor. Dengeli bir dağılım için protein %25–35 aralığı önerilir.",
            $"P %{p} · K %{c} · Y %{f}"));
    }

    private static void AddWorkoutInsight(List<InsightDto> list, List<WorkoutLog> logs)
    {
        if (logs.Count == 0)
        {
            list.Add(new InsightDto(InsightCategory.Workout, InsightSeverity.Warning, "Antrenman Sıklığı",
                $"Son {WorkoutWindowDays} günde kayıtlı antrenman yok. Haftada en az 3 antrenman; hem sağlık hem performans için önemli bir hedef.", "0 antrenman"));
            return;
        }

        var groups = logs.Where(l => l.Exercise.Category == ExerciseCategory.Strength)
            .GroupBy(l => l.Exercise.MuscleGroup)
            .ToDictionary(g => g.Key, g => g.Count());

        int CountOf(MuscleGroup g) => groups.TryGetValue(g, out var c) ? c : 0;

        var chest = CountOf(MuscleGroup.Chest);
        var back = CountOf(MuscleGroup.Back);
        var legs = CountOf(MuscleGroup.Legs);
        var metric = $"Göğüs {chest} · Sırt {back} · Bacak {legs}";

        var major = new[] { (MuscleGroup.Chest, chest), (MuscleGroup.Back, back), (MuscleGroup.Legs, legs) };
        var maxCount = major.Max(m => m.Item2);
        var neglected = major.Where(m => m.Item2 <= 1).Select(m => MuscleName(m.Item1)).ToList();

        if (maxCount >= 2 && neglected.Count > 0)
        {
            var names = string.Join(", ", neglected);
            list.Add(new InsightDto(InsightCategory.Workout, InsightSeverity.Warning, "Antrenman Dengesi",
                $"Son {WorkoutWindowDays} günde diğer bölgeler düzenli çalışılırken {names} antrenmanı yetersiz kalmış. Dengeli gelişim ve sakatlık önleme için bu grubu programına ekle.", metric));
        }
        else
        {
            list.Add(new InsightDto(InsightCategory.Workout, InsightSeverity.Positive, "Antrenman Dengesi",
                $"Son {WorkoutWindowDays} günde {logs.Count} antrenman kaydın var ve kas grupların dengeli çalışıyor. Bu istikrarı koru!", metric));
        }
    }

    private static void AddWeightInsight(List<InsightDto> list, User user, List<WeightEntry> entries)
    {
        if (entries.Count < 2) return;

        var first = entries.First();
        var last = entries.Last();
        var days = (last.RecordedAt - first.RecordedAt).TotalDays;
        if (days < 7) return;

        var deltaKg = last.WeightKg - first.WeightKg;
        var perWeek = Math.Round(deltaKg / (days / 7.0), 2);
        var metric = $"{(deltaKg >= 0 ? "+" : "")}{Math.Round(deltaKg, 1)} kg / {Math.Round(days)} gün";

        InsightSeverity severity;
        string msg;

        switch (user.GoalType)
        {
            case GoalType.LoseWeight:
                severity = deltaKg < -0.1 ? InsightSeverity.Positive : InsightSeverity.Warning;
                msg = deltaKg < -0.1
                    ? $"Son {Math.Round(days)} günde {Math.Abs(Math.Round(deltaKg, 1))} kg verdin (haftada ~{Math.Abs(perWeek)} kg). Kilo verme hedefinde doğru yoldasın."
                    : $"Son {Math.Round(days)} günde kilonda anlamlı bir düşüş görünmüyor. Kalori açığını ve kayıt tutarlılığını gözden geçirmek faydalı olabilir.";
                break;

            case GoalType.GainMuscle:
                severity = deltaKg > 0.05 ? InsightSeverity.Positive : InsightSeverity.Info;
                msg = deltaKg > 0.05
                    ? $"Son {Math.Round(days)} günde {Math.Round(deltaKg, 1)} kg aldın (haftada ~{perWeek} kg). Kas kazanımı için iyi; artış hızının haftada 0.25–0.5 kg'ı aşmamasına dikkat et."
                    : $"Kilon büyük ölçüde sabit kalmış. Kas kazanımı için hafif bir kalori fazlası gerekebilir.";
                break;

            default:
                severity = Math.Abs(deltaKg) < 1 ? InsightSeverity.Positive : InsightSeverity.Info;
                msg = Math.Abs(deltaKg) < 1
                    ? $"Kilon son {Math.Round(days)} günde stabil (~{Math.Round(last.WeightKg, 1)} kg). Bakım hedefin için ideal."
                    : $"Son {Math.Round(days)} günde {Math.Abs(Math.Round(deltaKg, 1))} kg {(deltaKg < 0 ? "kayıp" : "artış")} var. Bakım hedefin için takibe devam et.";
                break;
        }

        list.Add(new InsightDto(InsightCategory.Weight, severity, "Kilo Trendi", msg, metric));
    }

    private static void AddConsistencyInsight(List<InsightDto> list, List<NutritionLog> logs)
    {
        var loggedDays = logs.Select(l => l.LoggedAt.Date).Distinct().Count();
        if (loggedDays == 0) return;

        var metric = $"{loggedDays}/{NutritionWindowDays} gün";

        if (loggedDays >= NutritionWindowDays * 0.7)
        {
            list.Add(new InsightDto(InsightCategory.Consistency, InsightSeverity.Positive, "Kayıt Tutarlılığı",
                $"Son {NutritionWindowDays} günün {loggedDays} gününde beslenme kaydı girmişsin. Bu tutarlılık doğru analiz ve sürdürülebilir ilerleme için çok değerli.", metric));
        }
        else if (loggedDays <= NutritionWindowDays * 0.35)
        {
            list.Add(new InsightDto(InsightCategory.Consistency, InsightSeverity.Info, "Kayıt Tutarlılığı",
                $"Son {NutritionWindowDays} günde yalnızca {loggedDays} gün kayıt var. Daha düzenli kayıt, önerilerin isabetini belirgin şekilde artırır.", metric));
        }
    }

    private static string MuscleName(MuscleGroup group) => group switch
    {
        MuscleGroup.Chest => "göğüs",
        MuscleGroup.Back => "sırt",
        MuscleGroup.Legs => "bacak",
        MuscleGroup.Shoulders => "omuz",
        MuscleGroup.Arms => "kol",
        MuscleGroup.Core => "karın",
        _ => group.ToString().ToLowerInvariant()
    };
}
