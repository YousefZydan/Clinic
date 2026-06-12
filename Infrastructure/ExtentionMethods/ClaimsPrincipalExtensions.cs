using System.Security.Claims;

namespace Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}

