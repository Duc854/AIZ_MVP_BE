using Shared.Models;
using System.Security.Claims;

namespace AIZ_MVP_Presentation.Extensions
{
    public static class HttpContextExtensions
    {
        public static UserIdentity? GetUserIdentity(this HttpContext httpContext)
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return null;
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var usernameClaim = httpContext.User.FindFirst(ClaimTypes.Name);
            var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null) throw new InvalidOperationException("UserId claim is missing");
            if (usernameClaim == null) throw new InvalidOperationException("Username claim is missing");
            if (roleClaim == null) throw new InvalidOperationException("Role claim is missing");

            return new UserIdentity
            {
                UserId = userIdClaim.Value,
                Username = usernameClaim.Value,
                Role = roleClaim.Value
            };
        }
    }
}
