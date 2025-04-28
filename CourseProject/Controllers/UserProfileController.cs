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
        public async Task<IActionResult> DeleteTemplates(string templateIds)
        {
            var user = await _userManager.GetUserAsync(User);

            // Парсим ID шаблонов
            var ids = templateIds?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            // Валидация
            if (ids.Count == 0 || user == null)
            {
                TempData["Error"] = "Не выбрано ни одного шаблона для удаления";
                return RedirectToAction(nameof(Profile));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Удаление связанных данных
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

                // Непосредственное удаление шаблонов
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