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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .Include(t => t.Tags)
                    .ThenInclude(t => t.Tag)
                .Include(t => t.Topic)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null) return NotFound();

            // Исправление: Используем SelectList вместо списка Topic
            ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
            return View("Template", template);
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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    Template model,
    IFormFile ImageFile,
    bool removeImage = false,
    List<string> tags = null,
    [FromServices] IWebHostEnvironment hostingEnvironment = null)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
                return View("Template", model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Загрузка существующего шаблона с вопросами и опциями
                var existingTemplate = await _context.Templates
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Options)
                    .Include(t => t.Tags)
                        .ThenInclude(tt => tt.Tag)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existingTemplate == null || existingTemplate.AuthorId != user.Id)
                {
                    return NotFound();
                }

                // Обновление основных полей шаблона
                existingTemplate.Title = model.Title;
                existingTemplate.Description = model.Description;
                existingTemplate.TopicId = model.TopicId;
                existingTemplate.IsPublic = model.IsPublic;
                existingTemplate.UpdatedAt = DateTime.UtcNow;

                // Обработка изображения
                if (removeImage && !string.IsNullOrEmpty(existingTemplate.ImagePath))
                {
                    var oldImagePath = Path.Combine(hostingEnvironment.WebRootPath,
                        existingTemplate.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    existingTemplate.ImagePath = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
                    var uniqueFileName = Guid.NewGuid() + "_" + ImageFile.FileName;
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);

                    await using var fileStream = new FileStream(filePath, FileMode.Create);
                    await ImageFile.CopyToAsync(fileStream);

                    existingTemplate.ImagePath = "/uploads/" + uniqueFileName;
                }

                // Обновление вопросов
                foreach (var question in model.Questions)
                {
                    if (question.Id == 0) // Новый вопрос
                    {
                        // Добавляем новый вопрос
                        question.TemplateId = existingTemplate.Id;
                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync(); // Сохраняем для получения ID

                        // Добавляем опции нового вопроса
                        foreach (var option in question.Options)
                        {
                            if (!string.IsNullOrWhiteSpace(option.Text))
                            {
                                option.QuestionId = question.Id;
                                _context.QuestionOptions.Add(option);
                            }
                        }
                    }
                    else // Существующий вопрос
                    {
                        var existingQuestion = existingTemplate.Questions
                            .FirstOrDefault(q => q.Id == question.Id);

                        if (existingQuestion != null)
                        {
                            // Обновление полей вопроса
                            existingQuestion.Text = question.Text;
                            existingQuestion.Type = question.Type;
                            existingQuestion.IsRequired = question.IsRequired;
                            existingQuestion.Order = question.Order;

                            // Обработка опций
                            foreach (var option in question.Options)
                            {
                                if (option.Id == 0) // Новая опция
                                {
                                    if (!string.IsNullOrWhiteSpace(option.Text))
                                    {
                                        option.QuestionId = existingQuestion.Id;
                                        _context.QuestionOptions.Add(option);
                                    }
                                }
                                else // Обновление существующей опции
                                {
                                    var existingOption = existingQuestion.Options
                                        .FirstOrDefault(o => o.Id == option.Id);

                                    if (existingOption != null)
                                    {
                                        existingOption.Text = option.Text;
                                    }
                                }
                            }

                            // Удаление отсутствующих опций
                            var optionsToDelete = existingQuestion.Options
                                .Where(o => !question.Options.Any(qo => qo.Id == o.Id))
                                .ToList();

                            foreach (var option in optionsToDelete)
                            {
                                _context.QuestionOptions.Remove(option);
                            }
                        }
                    }
                }

                // Удаление вопросов, отсутствующих в модели
                var questionsToDelete = existingTemplate.Questions
                    .Where(q => !model.Questions.Any(mq => mq.Id == q.Id))
                    .ToList();

                foreach (var question in questionsToDelete)
                {
                    // Удаляем связанные опции
                    var options = await _context.QuestionOptions
                        .Where(o => o.QuestionId == question.Id)
                        .ToListAsync();

                    _context.QuestionOptions.RemoveRange(options);
                    _context.Questions.Remove(question);
                }

                // Обновление тегов
                var existingTags = existingTemplate.Tags.ToList();
                foreach (var existingTag in existingTags)
                {
                    if (!tags.Contains(existingTag.Tag.Name))
                    {
                        _context.TemplateTags.Remove(existingTag);
                    }
                }

                foreach (var tagName in tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
                {
                    if (!existingTags.Any(et => et.Tag.Name == tagName))
                    {
                        var tag = await _context.Tags
                            .FirstOrDefaultAsync(t => t.Name == tagName)
                            ?? new Tag { Name = tagName.Trim() };

                        if (tag.Id == 0)
                        {
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }

                        _context.TemplateTags.Add(new TemplateTag
                        {
                            TemplateId = existingTemplate.Id,
                            TagId = tag.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
                return View("Template", model);
            }
        }
    }
}
    
