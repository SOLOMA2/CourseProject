using CourseProject.DataUser;
using CourseProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ManagerMyAnswersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppUserDbContext _context;

    public ManagerMyAnswersController(UserManager<AppUser> userManager, AppUserDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> MyAnswers()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var responses = await _context.FormResponses
            .Where(r => r.UserId == user.Id)
            .Include(r => r.Template)
            .ThenInclude(t => t.Questions)
            .ThenInclude(q => q.Options)
            .Include(r => r.Answers)
            .ThenInclude(a => a.SelectedOptions)
            .ThenInclude(so => so.QuestionOption)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return View(responses);
    }
}