using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Profile;
using FitMetrics.Application.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ProfileService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);

        user.FullName = request.FullName.Trim();
        user.Age = request.Age;
        user.Gender = request.Gender;
        user.HeightCm = request.HeightCm;
        user.CurrentWeightKg = request.CurrentWeightKg;
        user.ActivityLevel = request.ActivityLevel;
        user.GoalType = request.GoalType;
        user.TargetWeightKg = request.TargetWeightKg;

        var tdee = HealthCalculator.CalculateTdee(
            request.Gender, request.CurrentWeightKg, request.HeightCm, request.Age, request.ActivityLevel);

        // Kullanıcı manuel hedef verdiyse onu kullan, vermediyse yeniden hesapla
        user.DailyCalorieGoal = request.DailyCalorieGoal ?? HealthCalculator.CalculateCalorieGoal(request.GoalType, tdee);
        user.DailyProteinGoal = request.DailyProteinGoal ?? HealthCalculator.CalculateProteinGoal(request.GoalType, request.CurrentWeightKg);
        user.DailyWaterGoalMl = request.DailyWaterGoalMl ?? user.DailyWaterGoalMl;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }
}
