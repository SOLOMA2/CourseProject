using CourseProject.DataUser;
using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagerController : Controller
    {
        private readonly AppUserDbContext _db;
        private readonly UserManager<AppUser> _users;
        private readonly SignInManager<AppUser> _signIn;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly ILogger<UserManagerController> _log;

        public UserManagerController(
            AppUserDbContext db,
            UserManager<AppUser> users,
            RoleManager<IdentityRole> roles,
            SignInManager<AppUser> signIn,
            ILogger<UserManagerController> log)
        {
            _db = db;
            _users = users;
            _roles = roles;
            _signIn = signIn;
            _log = log;
        }

        public async Task<IActionResult> AdminPanel(
    string? search,
    int pageNumber = 1,
    int pageSize = 10)
        {
            var query = _users.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var usersPage = await query
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vmList = new List<UserWithRolesVM>(usersPage.Count);
            foreach (var u in usersPage)
            {
                var roles = await _users.GetRolesAsync(u);
                vmList.Add(new UserWithRolesVM
                {
                    User = u,
                    Roles = roles.ToList() // Явное преобразование IList -> List
                });
            }

            ViewBag.AllRoles = await _roles.Roles
                .Select(r => r.Name)
                .ToListAsync();

            var model = new AdminPanel
            {
                Users = vmList,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SearchTerm = search
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers(
    List<string> userIds,
    int pageNumber = 1,
    string? search = null,
    int pageSize = 10)
        {
            if (userIds == null || !userIds.Any())
                return BadRequest("Не выбраны пользователи для удаления.");

            var currentId = _users.GetUserId(User);
            var toDelete = await LoadUsersWithDependencies(userIds);

            if (!toDelete.Any())
                return RedirectToAction(nameof(AdminPanel), new { pageNumber, search, pageSize });

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var user in toDelete)
                {
                    var result = await _users.DeleteAsync(user); // Используем UserManager
                    if (!result.Succeeded)
                        throw new Exception(string.Join(", ", result.Errors));
                }

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _log.LogError(ex, "Ошибка при удалении пользователей: {Ids}", userIds);
                TempData["Error"] = "Не удалось удалить пользователей. Смотрите логи.";
                return RedirectToAction(nameof(AdminPanel), new { pageNumber, search, pageSize });
            }

            if (toDelete.Any(u => u.Id == currentId))
            {
                await _signIn.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            TempData["Message"] = $"Удалено пользователей: {toDelete.Count}";
            return RedirectToAction(nameof(AdminPanel), new { pageNumber, search, pageSize });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUsers(
            List<string> userIds,
            int pageNumber = 1,
            string? search = null,
            int pageSize = 10)
        {
            if (userIds == null || !userIds.Any())
                return BadRequest();

            var currentId = _users.GetUserId(User);
            var users = await _users.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var u in users)
            {
                u.LockoutEnd = DateTimeOffset.MaxValue;
                u.LockoutEnabled = true;
            }

            await _db.SaveChangesAsync();

            if (users.Any(u => u.Id == currentId))
            {
                await _signIn.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(AdminPanel),
                new { pageNumber, search, pageSize });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUsers(
            List<string> userIds,
            int pageNumber = 1,
            string? search = null,
            int pageSize = 10)
        {
            if (userIds == null || !userIds.Any())
                return BadRequest();

            var currentId = _users.GetUserId(User);
            var users = await _users.Users
                .Where(u => userIds.Contains(u.Id) && u.Id != currentId)
                .ToListAsync();

            foreach (var u in users)
            {
                u.LockoutEnd = null;
                u.LockoutEnabled = false;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel),
                new { pageNumber, search, pageSize });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(
            string userId,
            List<string> selectedRoles,
            int pageNumber,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId required");

            var user = await _users.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var currentRoles = await _users.GetRolesAsync(user);
                await _users.RemoveFromRolesAsync(user, currentRoles);

                if (selectedRoles?.Any() == true)
                    await _users.AddToRolesAsync(user, selectedRoles);

                await tx.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _log.LogError(ex, "Не удалось обновить роли для {UserId}", userId);
                return StatusCode(500, "Internal error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var user = await _users.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _users.GetRolesAsync(user);
            return Json(roles);
        }

        private Task<List<AppUser>> LoadUsersWithDependencies(List<string> userIds)
        {
            return _db.Users
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Templates!)
                .ThenInclude(t => t.Questions!)
                .ThenInclude(q => q.Options!)
                .ThenInclude(o => o.SelectedOptions!)
                .Include(u => u.Templates)
                .ThenInclude(t => t.Responses!)
                .ThenInclude(r => r.Answers!)
                .ThenInclude(a => a.SelectedOptions!)
                .AsTracking()
                .ToListAsync();
        }
    }
}
