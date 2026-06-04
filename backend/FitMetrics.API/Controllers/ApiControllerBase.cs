using System.Security.Claims;
using FitMetrics.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>JWT'den çözülen oturum açmış kullanıcının kimliği.</summary>
    protected int UserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedException("Geçersiz veya eksik oturum bilgisi.");
        }
    }
}
