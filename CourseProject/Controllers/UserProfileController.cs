using CourseProject.DataUser;
using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CourseProject.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly AppUserDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserProfileController(AppUserDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var templates = await _context.Templates
                .Include(t => t.Author)    
                .Include(t => t.Topic)
                .Include(t => t.Questions)
                            .Include(t => t.Likes) // Включаем лайки
            .Include(t => t.Views) // Включаем просмотры
                .Where(t => t.AuthorId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking() 
                .ToListAsync();

            return View(new UserProfileVМ
            {
                User = user,
                Templates = templates
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplates(List<int> templateIds)
        {
            var user = await _userManager.GetUserAsync(User);

            await _context.Templates
                .Where(t => templateIds.Contains(t.Id) && t.AuthorId == user.Id)
                .ExecuteDeleteAsync();

            return RedirectToAction(nameof(Profile));
        }
    }
}