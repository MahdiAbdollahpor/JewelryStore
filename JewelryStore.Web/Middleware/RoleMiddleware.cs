using JewelryStore.Data.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JewelryStore.Web.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleMiddleware> _logger;

        public RoleMiddleware(RequestDelegate next, ILogger<RoleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                    if (user != null)
                    {
                        var currentRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        var dbRole = user.Role.ToString();

                        // اگر نقش در دیتابیس با نقش موجود در کوکی متفاوت بود، کوکی را Refresh کن
                        if (currentRole != dbRole)
                        {
                            _logger.LogInformation($"نقش کاربر {userId} از {currentRole} به {dbRole} تغییر کرد.");

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim(ClaimTypes.Name, user.Username),
                                new Claim(ClaimTypes.Role, dbRole)
                            };

                            var claimsIdentity = new ClaimsIdentity(
                                claims,
                                context.User.Identity.AuthenticationType);

                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                            };

                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            await context.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
