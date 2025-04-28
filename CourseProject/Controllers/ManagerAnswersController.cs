using CourseProject.DataUser;
using CourseProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; 

namespace CourseProject.Controllers
{
    public class ManagerAnswersController : Controller
    {
        private readonly AppUserDbContext _context;
        private readonly ILogger<UserManagerController> _logger;
        private readonly UserManager<AppUser> _userManager;


        public ManagerAnswersController(AppUserDbContext context, ILogger<UserManagerController> logger, UserManager<AppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> AllAnswers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var templates = await _context.Templates
                .Where(t => t.AuthorId == userId)
                .Include(t => t.Questions) // Включаем вопросы
                    .ThenInclude(q => q.Options) // Для каждого вопроса включаем его Options
                .Include(t => t.Responses) // Включаем ответы на шаблон
                    .ThenInclude(r => r.Answers) // Для каждого ответа включаем Answers
                        .ThenInclude(a => a.SelectedOptions) // Для каждого ответа включаем SelectedOptions
                            .ThenInclude(so => so.QuestionOption) // Для SelectedOption включаем QuestionOption
                .Include(t => t.Responses)
                    .ThenInclude(r => r.User) // Включаем пользователя для каждого ответа
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(templates);
        }
    }
}
