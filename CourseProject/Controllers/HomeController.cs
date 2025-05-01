using System.Diagnostics;
using CourseProject.Models;
using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models.MainModelViews;
using CourseProject.Models.ViewsModels.MainPageModel;
using Microsoft.AspNetCore.Mvc;
using CourseProject.DataUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppUserDbContext _context;
        private readonly ILogger<UserManagerController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(AppUserDbContext context, ILogger<UserManagerController> logger, UserManager<AppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchQuery, string tag)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id;

                IQueryable<Template> baseQuery = _context.Templates
                    .Include(t => t.Author)
                    .Include(t => t.Tags)
                     .ThenInclude(tt => tt.Tag)
                    .Include(t => t.Likes)
                    .Include(t => t.Views)
                    .Include(t => t.Questions)
                    .Include(t => t.Comments)
                    .Include(t => t.AllowedUsers) 
                    .AsQueryable();

                if (userId == null)
                {
                    baseQuery = baseQuery.Where(t => t.AccessType == AccessType.Public);
                }
                else
                {
                    baseQuery = baseQuery.Where(t =>
                        t.AccessType == AccessType.Public ||
                        t.AccessType == AccessType.Link ||
                        (t.AccessType == AccessType.Private &&
                         t.AllowedUsers.Any(au => au.UserId == userId)));
                }

                if (!string.IsNullOrEmpty(tag))
                {
                    baseQuery = baseQuery.Where(t =>
                        t.Tags.Any(tt => tt.Tag.Name == tag));
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    var cleanQuery = searchQuery.Trim().ToLower();
                    baseQuery = baseQuery.Where(t =>
                        t.Title.ToLower().Contains(cleanQuery) ||
                        t.Description.ToLower().Contains(cleanQuery) ||
                        t.Tags.Any(tt => tt.Tag.Name.ToLower().Contains(cleanQuery)));
                }

                var popularTemplates = await baseQuery
                    .OrderByDescending(t => t.Likes.Count)
                    .ThenByDescending(t => t.Views.Count)
                    .Take(6)
                    .ToListAsync();

                var recentTemplates = await baseQuery
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(6)
                    .ToListAsync();

                if (userId != null)
                {
                    foreach (var template in popularTemplates.Concat(recentTemplates))
                    {
                        template.IsLikedByCurrentUser = template.Likes
                            .Any(l => l.UserId == userId);
                    }
                }

                var popularTags = await _context.Tags
                    .Where(t => t.TemplateTags.Any()) 
                    .Select(t => new TagInfo
                    {
                        Tag = t,
                        UsageCount = t.TemplateTags.Count
                    })
                    .OrderByDescending(t => t.UsageCount)
                    .Take(20)
                    .ToListAsync();

                var model = new HomeIndexViewModel
                {
                    PopularTemplates = popularTemplates,
                    RecentTemplates = recentTemplates,
                    PopularTags = popularTags,
                    SearchQuery = searchQuery,
                    Tag = tag,
                    SearchResults = (!string.IsNullOrEmpty(searchQuery) || !string.IsNullOrEmpty(tag))
                        ? await baseQuery.ToListAsync()
                        : null
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главной страницы");
                return View(new HomeIndexViewModel
                {
                    PopularTemplates = new List<Template>(),
                    RecentTemplates = new List<Template>(),
                    PopularTags = new List<TagInfo>()
                });
            }
        }

    }
}
