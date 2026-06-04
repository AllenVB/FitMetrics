using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Workouts;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public WorkoutService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ExerciseDto>> GetExercisesAsync(string? search, CancellationToken ct = default)
    {
        var query = _db.Exercises.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => e.Name.Contains(term));
        }

        var exercises = await query.OrderBy(e => e.Name).ToListAsync(ct);
        return exercises.Select(e => _mapper.Map<ExerciseDto>(e)).ToList();
    }

    public async Task<WorkoutLogDto> AddLogAsync(int userId, CreateWorkoutLogRequest request, CancellationToken ct = default)
    {
        var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == request.ExerciseId, ct)
                       ?? throw new NotFoundException("Egzersiz", request.ExerciseId);

        // Süre verilmişse onu, yoksa set başına ~3 dk varsayımıyla tahmini süreyi kullan
        var effectiveMinutes = request.DurationMinutes ?? (request.Sets.HasValue ? request.Sets.Value * 3 : 0);
        var caloriesBurned = Math.Round(exercise.CaloriesBurnedPerMinute * effectiveMinutes, 1);

        var log = new WorkoutLog
        {
            UserId = userId,
            ExerciseId = exercise.Id,
            DurationMinutes = request.DurationMinutes,
            Sets = request.Sets,
            Reps = request.Reps,
            WeightKg = request.WeightKg,
            CaloriesBurned = caloriesBurned,
            PerformedAt = request.PerformedAt ?? DateTime.UtcNow
        };

        _db.WorkoutLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        return ToDto(log, exercise);
    }

    public async Task DeleteLogAsync(int userId, int logId, CancellationToken ct = default)
    {
        var log = await _db.WorkoutLogs.FirstOrDefaultAsync(w => w.Id == logId && w.UserId == userId, ct)
                  ?? throw new NotFoundException("Antrenman kaydı", logId);

        _db.WorkoutLogs.Remove(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<WorkoutLogDto>> GetLogsAsync(int userId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow.AddDays(1);

        var logs = await _db.WorkoutLogs
            .Include(w => w.Exercise)
            .Where(w => w.UserId == userId && w.PerformedAt >= fromDate && w.PerformedAt < toDate)
            .OrderByDescending(w => w.PerformedAt)
            .ToListAsync(ct);

        return logs.Select(l => ToDto(l, l.Exercise)).ToList();
    }

    private static WorkoutLogDto ToDto(WorkoutLog log, Exercise exercise) => new(
        log.Id,
        exercise.Id,
        exercise.Name,
        exercise.Category,
        exercise.MuscleGroup,
        log.DurationMinutes,
        log.Sets,
        log.Reps,
        log.WeightKg,
        log.CaloriesBurned,
        log.PerformedAt);
}
