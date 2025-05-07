using CourseProject.DataUser;
using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly AppUserDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserProfileController(
            AppUserDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();  

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var templates = await _context.Templates
                .AsNoTracking()
                .Where(t => isAdmin || t.AuthorId == currentUser.Id)
                .Include(t => t.Topic)           
                .ToListAsync();

            return View(new UserProfileVМ
            {
                User = currentUser,
                Templates = templates
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplates(List<int> templateIds)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            if (templateIds == null || templateIds.Count == 0)
            {
                TempData["Error"] = "Не выбрано ни одного шаблона.";
                return RedirectToAction(nameof(Profile));
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var templatesToDelete = await _context.Templates
                .Where(t => templateIds.Contains(t.Id)
                         && (isAdmin || t.AuthorId == currentUser.Id))
                .ToListAsync();

            if (templatesToDelete.Count == 0)
            {
                TempData["Error"] = "Нет доступных для удаления шаблонов.";
                return RedirectToAction(nameof(Profile));
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Templates.RemoveRange(templatesToDelete);
                var deletedCount = await _context.SaveChangesAsync();

                await tx.CommitAsync();

                TempData["Message"] = $"Успешно удалено шаблонов: {deletedCount}";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}
