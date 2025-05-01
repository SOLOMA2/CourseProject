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
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var templates = await _context.Templates
                .Include(t => t.Author)
                .Include(t => t.Topic)
                .Include(t => t.Likes)
                .Include(t => t.Views)
                .Where(t => isAdmin || t.AuthorId == user.Id)
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
        public async Task<IActionResult> DeleteTemplates(string templateIds)
        {
            var user = await _userManager.GetUserAsync(User);

            var ids = templateIds?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            if (ids.Count == 0 || user == null)
            {
                TempData["Error"] = "Не выбрано ни одного шаблона для удаления";
                return RedirectToAction(nameof(Profile));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.TemplateAccess
                    .Where(ta => ids.Contains(ta.TemplateId))
                    .ExecuteDeleteAsync();

                await _context.SelectedOptions
                    .Where(so => so.Answer.UserResponse.Template.AuthorId == user.Id
                        && ids.Contains(so.Answer.UserResponse.TemplateId))
                    .ExecuteDeleteAsync();

                await _context.Answers
                    .Where(a => a.UserResponse.Template.AuthorId == user.Id
                        && ids.Contains(a.UserResponse.TemplateId))
                    .ExecuteDeleteAsync();

                await _context.FormResponses
                    .Where(fr => fr.Template.AuthorId == user.Id
                        && ids.Contains(fr.TemplateId))
                    .ExecuteDeleteAsync();

                var deletedCount = await _context.Templates
                    .Where(t => ids.Contains(t.Id) && t.AuthorId == user.Id)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();

                TempData["Message"] = $"Успешно удалено шаблонов: {deletedCount}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}