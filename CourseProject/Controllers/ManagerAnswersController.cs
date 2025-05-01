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
                .Include(t => t.Questions) 
                .ThenInclude(q => q.Options) 
                .Include(t => t.Responses) 
                .ThenInclude(r => r.Answers) 
                .ThenInclude(a => a.SelectedOptions) 
                .ThenInclude(so => so.QuestionOption) 
                .Include(t => t.Responses)
                .ThenInclude(r => r.User) 
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(templates);
        }
    }
}
