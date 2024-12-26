using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ComHub.Shared.Services.Utils;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly ClaimsPrincipal _claimsPrincipal =
        httpContextAccessor.HttpContext?.User
        ?? throw new Exception("No active IHttpContextAccessor.HttpContext");

    public int UserId
    {
        get
        {
            var userIdString = _claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userIdString) ? default : int.Parse(userIdString);
        }
    }

    public string UserRole
    {
        get
        {
            var role = _claimsPrincipal?.FindFirstValue(ClaimTypes.Role);
            return role ?? "";
        }
    }
}
