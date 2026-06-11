using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Workouts;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly IApplicationDbContext _db;

    public WorkoutPlanService(IApplicationDbContext db) => _db = db;

    public async Task<List<WorkoutPlanSummaryDto>> GetAllAsync(int userId, CancellationToken ct = default)
    {
        return await _db.WorkoutPlans
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new WorkoutPlanSummaryDto(
                p.Id,
                p.Name,
                p.CreatedAt,
                p.Days.SelectMany(d => d.Exercises).Count()))
            .ToListAsync(ct);
    }

    public async Task<WorkoutPlanDto> GetByIdAsync(int userId, int planId, CancellationToken ct = default)
    {
        var plan = await LoadPlanAsync(userId, planId, ct);
        return ToDto(plan);
    }

    public async Task<WorkoutPlanDto> CreateAsync(int userId, CreateWorkoutPlanRequest req, CancellationToken ct = default)
    {
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Name   = req.Name.Trim()
        };

        foreach (var dayReq in req.Days.Where(d => d.Exercises.Count > 0))
        {
            var day = new WorkoutPlanDay { DayIndex = dayReq.DayIndex };
            foreach (var exReq in dayReq.Exercises)
                day.Exercises.Add(new WorkoutPlanExercise
                {
                    ExerciseId      = exReq.ExerciseId,
                    Sets            = exReq.Sets,
                    Reps            = exReq.Reps,
                    DurationMinutes = exReq.DurationMinutes,
                    SortOrder       = exReq.SortOrder
                });
            plan.Days.Add(day);
        }

        _db.WorkoutPlans.Add(plan);
        await _db.SaveChangesAsync(ct);

        return ToDto(await LoadPlanAsync(userId, plan.Id, ct));
    }

    public async Task<WorkoutPlanDto> UpdateAsync(int userId, int planId, CreateWorkoutPlanRequest req, CancellationToken ct = default)
    {
        var plan = await _db.WorkoutPlans
            .Include(p => p.Days).ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, ct)
            ?? throw new NotFoundException("Program", planId);

        plan.Name = req.Name.Trim();

        // Tüm mevcut günleri sil, yenilerini ekle (cascade)
        _db.WorkoutPlanDays.RemoveRange(plan.Days);
        plan.Days.Clear();

        foreach (var dayReq in req.Days.Where(d => d.Exercises.Count > 0))
        {
            var day = new WorkoutPlanDay { DayIndex = dayReq.DayIndex };
            foreach (var exReq in dayReq.Exercises)
                day.Exercises.Add(new WorkoutPlanExercise
                {
                    ExerciseId      = exReq.ExerciseId,
                    Sets            = exReq.Sets,
                    Reps            = exReq.Reps,
                    DurationMinutes = exReq.DurationMinutes,
                    SortOrder       = exReq.SortOrder
                });
            plan.Days.Add(day);
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(await LoadPlanAsync(userId, planId, ct));
    }

    public async Task DeleteAsync(int userId, int planId, CancellationToken ct = default)
    {
        var plan = await _db.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, ct)
            ?? throw new NotFoundException("Program", planId);

        _db.WorkoutPlans.Remove(plan);
        await _db.SaveChangesAsync(ct);
    }

    // ── Yardımcılar ──────────────────────────────────────────────────────────

    private async Task<WorkoutPlan> LoadPlanAsync(int userId, int planId, CancellationToken ct) =>
        await _db.WorkoutPlans
            .Include(p => p.Days)
                .ThenInclude(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, ct)
        ?? throw new NotFoundException("Program", planId);

    private static WorkoutPlanDto ToDto(WorkoutPlan p) => new(
        p.Id, p.Name, p.CreatedAt,
        p.Days
            .OrderBy(d => d.DayIndex)
            .Select(d => new WorkoutPlanDayDto(
                d.Id, d.DayIndex,
                d.Exercises
                    .OrderBy(e => e.SortOrder)
                    .Select(e => new WorkoutPlanExerciseDto(
                        e.Id,
                        e.ExerciseId,
                        e.Exercise.Name,
                        e.Exercise.MuscleGroup.ToString(),
                        e.Exercise.Category.ToString(),
                        e.Sets, e.Reps, e.DurationMinutes, e.SortOrder))
                    .ToList()))
            .ToList());
}
