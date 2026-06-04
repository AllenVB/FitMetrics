using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Profile;

namespace FitMetrics.Application.Services.Interfaces;

public interface IProfileService
{
    Task<UserDto> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default);
}
