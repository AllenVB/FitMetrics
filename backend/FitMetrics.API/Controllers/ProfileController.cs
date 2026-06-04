using FitMetrics.Application.DTOs.Auth;
using FitMetrics.Application.DTOs.Profile;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class ProfileController : ApiControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService) => _profileService = profileService;

    [HttpGet]
    public async Task<ActionResult<UserDto>> Get(CancellationToken ct)
        => Ok(await _profileService.GetProfileAsync(UserId, ct));

    [HttpPut]
    public async Task<ActionResult<UserDto>> Update(UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _profileService.UpdateProfileAsync(UserId, request, ct));
}
