using FitMetrics.Application.Common.Exceptions;
using FitMetrics.Application.Common.Helpers;
using FitMetrics.Application.Common.Interfaces;
using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.Services.Interfaces;
using FitMetrics.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace FitMetrics.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(IApplicationDbContext db, IPasswordHasher hasher, ITokenService tokenService, IMapper mapper)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Bu e-posta adresi zaten kayıtlı.");

        var tdee = HealthCalculator.CalculateTdee(
            request.Gender, request.CurrentWeightKg, request.HeightCm, request.Age, request.ActivityLevel);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(request.Password),
            Age = request.Age,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            CurrentWeightKg = request.CurrentWeightKg,
            ActivityLevel = request.ActivityLevel,
            GoalType = request.GoalType,
            TargetWeightKg = request.TargetWeightKg,
            DailyCalorieGoal = HealthCalculator.CalculateCalorieGoal(request.GoalType, tdee),
            DailyProteinGoal = HealthCalculator.CalculateProteinGoal(request.GoalType, request.CurrentWeightKg)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        // Başlangıç kilo kaydı — ilerleme grafiğinin ilk noktası
        _db.WeightEntries.Add(new WeightEntry
        {
            UserId = user.Id,
            WeightKg = request.CurrentWeightKg,
            RecordedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("E-posta veya parola hatalı.");

        return BuildAuthResponse(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException("Kullanıcı", userId);
        return _mapper.Map<UserDto>(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAt, _mapper.Map<UserDto>(user));
    }
}
