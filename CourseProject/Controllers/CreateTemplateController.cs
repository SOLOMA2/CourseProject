using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models.MainModelViews;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using CourseProject.Models;
using CourseProject.DataUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CourseProject.Controllers
{
    public class CreateTemplateController : Controller
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppUserDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<UserManagerController> _logger;
        private readonly IConfiguration _config;


        public CreateTemplateController(
    AppUserDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    SignInManager<AppUser> signInManager,
    ILogger<UserManagerController> logger,
    IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        //[HttpGet]
        //public IActionResult Template()
        //{
        //    return View();
        //}
        [HttpGet]
        public IActionResult Template()
        {
            var model = new Template
            {
                Questions = new List<Question> { new Question() }, // Инициализация первого вопроса
                Tags = new List<TemplateTag>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Template(
    Template template,
    IFormFile ImageFile,
    List<string> tags,
    [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            template.AuthorId = user.Id;
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
                return View(template);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Сохраняем изображение
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
                    var uniqueFileName = Guid.NewGuid() + "_" + ImageFile.FileName;
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);

                    await using var fileStream = new FileStream(filePath, FileMode.Create);
                    await ImageFile.CopyToAsync(fileStream);

                    template.ImagePath = "/uploads/" + uniqueFileName;
                }

                // 2. Сохраняем шаблон
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();

                // 3. Сохраняем вопросы с опциями
                // 3. Сохраняем вопросы с опциями
                foreach (var question in template.Questions)
                {
                    question.TemplateId = template.Id;

                    // Для новых вопросов
                    if (question.Id == 0)
                    {
                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync();
                    }

                    // Обновляем существующие и добавляем новые опции
                    foreach (var option in question.Options)
                    {
                        if (string.IsNullOrWhiteSpace(option.Text)) continue;

                        if (option.Id == 0)
                        {
                            option.QuestionId = question.Id;
                            _context.QuestionOptions.Add(option);
                        }
                        else
                        {
                            _context.QuestionOptions.Update(option);
                        }
                    }
                }
                await _context.SaveChangesAsync();

                // 4. Обработка тегов
                foreach (var tagName in tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name == tagName)
                        ?? new Tag { Name = tagName.Trim() };

                    if (tag.Id == 0)
                    {
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    if (!_context.TemplateTags.Any(tt =>
                        tt.TemplateId == template.Id && tt.TagId == tag.Id))
                    {
                        _context.TemplateTags.Add(new TemplateTag
                        {
                            TemplateId = template.Id,
                            TagId = tag.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
                return View(template);
            }
        }
    }



    }
    
