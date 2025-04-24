using Azure;
using CourseProject.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CourseProject.CustomMiddleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CustomAdminMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAdminMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();

            if (controllerName != "UserManager" && controllerName != "AdminPanel")
            {
                await _next(context);
                return;
            }

            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var isLockedOut = await userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Home/Index");
                    return;
                }
            }
            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomAdminMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAdminMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomAdminMiddleware>();
        }
    }
}
