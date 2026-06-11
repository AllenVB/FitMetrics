using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    // Test modunda auth devre dışı — UserId her zaman 1 (ilk kullanıcı)
    protected int UserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 1;
        }
    }
}
