using CourseProject.DataUser;
using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Runtime.InteropServices;

namespace CourseProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagerController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppUserDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<UserManagerController> _logger;

        public UserManagerController(
            AppUserDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            ILogger<UserManagerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> AdminPanelAsync(string search, int? pageNumber, int pageSize = 10)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            pageNumber ??= 1;
            var count = await usersQuery.CountAsync();
            var usersList = await usersQuery
                .Skip((pageNumber.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var usersWithRoles = new List<UserWithRolesVM>();
            foreach (var user in usersList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesVM
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            var model = new AdminPanel
            {
                Users = usersWithRoles,
                PageIndex = pageNumber.Value,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                SearchTerm = search
            };

            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUsers(List<string> userIds, int pageNumber = 1, string search = "", int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool currentUserDeleted = false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var userId in userIds)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null) continue;

                    var userTemplates = await _context.Templates
                        .Where(t => t.AuthorId == userId)
                        .ToListAsync();

                    foreach (var template in userTemplates)
                    {
                        await _context.TemplateAccess
                            .Where(ta => ta.TemplateId == template.Id)
                            .ExecuteDeleteAsync();

                        await _context.Comments
                            .Where(c => c.TemplateId == template.Id)
                            .ExecuteDeleteAsync();

                        await _context.Likes
                            .Where(l => l.TemplateId == template.Id)
                            .ExecuteDeleteAsync();

                        await _context.Views
                            .Where(v => v.TemplateId == template.Id)
                            .ExecuteDeleteAsync();

                        _context.Templates.Remove(template);
                    }

                    await _context.Comments
                        .Where(c => c.AuthorId == userId)
                        .ExecuteDeleteAsync();

                    await _context.Likes
                        .Where(l => l.UserId == userId)
                        .ExecuteDeleteAsync();

                    await _context.TemplateAccess
                        .Where(ta => ta.UserId == userId)
                        .ExecuteDeleteAsync();

                    await _context.FormResponses
                        .Where(fr => fr.UserId == userId)
                        .ExecuteDeleteAsync();

                    await _userManager.DeleteAsync(user);

                    if (user.Id == currentUser?.Id)
                    {
                        currentUserDeleted = true;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("AdminPanel", new { pageNumber, search, pageSize });
            }

            if (currentUserDeleted)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            TempData["Message"] = $"Успешно удалено пользователей: {userIds.Count}";
            return RedirectToAction("AdminPanel", new { pageNumber, search, pageSize });
        }

        [HttpPost]
        public async Task<IActionResult> BlockUsers(List<string> userIds, int pageNumber = 1, string search = "", int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool curDeletUser = false;
            foreach (var id in userIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.LockoutEnd = DateTime.MaxValue;
                    user.LockoutEnabled = true;
                    await _userManager.UpdateAsync(user);

                    if(user.Id == currentUser?.Id)
                    {
                        curDeletUser = true;
                    }
                }
            }
            if (curDeletUser)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("AdminPanel", new { pageNumber, search, pageSize });

        }

        [HttpPost]
        public async Task<IActionResult> UnblockUsers(List<string> userIds, int pageNumber = 1, string search = "", int pageSize = 10)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool curDeletUser = false;
            foreach (var id in userIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (currentUser.Id == user.Id) continue;
                if (user != null)
                {
                    user.LockoutEnd = null;
                    user.LockoutEnabled = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            return RedirectToAction("AdminPanel", new { pageNumber, search, pageSize });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(
            string userId,
            List<string> selectedRoles,
            int pageNumber,
            string search)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("UserId: {userId}", userId); 
                return BadRequest("User ID is required");
            }

            var user = await _userManager.FindByIdAsync(userId); 
            if (user == null) return NotFound("User not found");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Error removing roles");
                    }

                    if (selectedRoles?.Any() == true)
                    {
                        var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                        if (!addResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return StatusCode(500, "Error adding roles");
                        }
                    }

                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Role update failed");
                    return StatusCode(500, "Internal error");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Json(roles);
        }

    }
}
