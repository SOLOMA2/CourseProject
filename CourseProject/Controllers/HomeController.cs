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

        // ����������� ��� ����������� HomeController.cs
        // ����������� ��� ����������� HomeController.cs
        public async Task<IActionResult> Index(string searchQuery)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id;

                // ������� ������ � ���������� ���� ������������
                var query = _context.Templates
            .Include(t => t.Author)
            .Include(t => t.Tags)
                .ThenInclude(tt => tt.Tag)
            .Include(t => t.Likes) // �������� �����
            .Include(t => t.Views) // �������� ���������
            .AsQueryable();

                // ����� (���������������� ��� �������������)
                // if (!string.IsNullOrEmpty(searchQuery))
                // {
                //     query = query.Where(t => 
                //         t.Title.Contains(searchQuery) || 
                //         t.Description.Contains(searchQuery) ||
                //         t.Tags.Any(tt => tt.Tag.Name.Contains(searchQuery)));
                // }

                var templates = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                // �������� ����� �������� ������������
                if (userId != null)
                {
                    foreach (var template in templates)
                    {
                        template.IsLikedByCurrentUser = template.Likes
                            .Any(l => l.UserId == userId);
                    }
                }

                // �������� ���������� ����
                var popularTags = await _context.Tags
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
                    PopularTemplates = templates.Take(6),
                    RecentTemplates = templates.OrderByDescending(t => t.UpdatedAt).Take(6),
                    PopularTags = popularTags,
                    SearchQuery = searchQuery
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� �������� ������� ��������");
                return View(new HomeIndexViewModel());
            }
        }


        private async Task<List<TagInfo>> GetPopularTagsAsync()
        {
            return await _context.Tags
                .Include(t => t.TemplateTags)
                .Select(t => new TagInfo
                {
                    Tag = t,
                    UsageCount = t.TemplateTags.Count
                })
                .OrderByDescending(t => t.UsageCount)
                .Take(20)
                .ToListAsync();
        }
    }
}
