using CourseProject.CustomMiddleware;
using CourseProject.DataUser;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppUserDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppUserDbContext>().
AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Необязательно: время жизни при "Запомнить меня"
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // Важно: куки будут удаляться при закрытии браузера
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = null; // Сессионная кука (удаляется при закрытии браузера)
});

var app = builder.Build();

await SeedServices.SeedDatabase(app.Services, app.Configuration);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<LikeHub>("/likeHub");
app.MapHub<ViewHub>("/viewHub");
app.MapHub<CommentHub>("/commentHub");


app.UseCustomAdminMiddleware();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
