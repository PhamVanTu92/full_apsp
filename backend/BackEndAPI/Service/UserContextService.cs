using System.Security.Claims;

namespace BackEndAPI.Service;

public class UserContextService(IHttpContextAccessor httpContextAccessor)
{
    public int UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : -1;
        }
    }

    public string Email => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string Role => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}