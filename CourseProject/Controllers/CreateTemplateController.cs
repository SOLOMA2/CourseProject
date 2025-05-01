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
        private readonly CloudinaryService _cloudinary;
        private readonly ILogger<UserManagerController> _logger;
        private readonly IConfiguration _config;

        public CreateTemplateController(
            AppUserDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            ILogger<UserManagerController> logger,
            IConfiguration config ,
            CloudinaryService cloudinary)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
            _cloudinary = cloudinary;
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

            ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
            return View("Template", template);
        }

        [HttpGet]
        public IActionResult Template()
        {
            var model = new Template
            {
                Questions = new List<Question> { new Question() }, 
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
            IFormFile? ImageFile,
            List<string> tags,
            List<string> selectedUserIds)
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
                    template.ImagePath = await _cloudinary.UploadImageAsync(ImageFile);
                }

                // 2. Сохраняем шаблон
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();

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
                //Пускай пока висит хз что делать...
                //template.LinkKey = template.AccessType == AccessType.Link
                //    ? Guid.NewGuid()
                //    : Guid.Empty;
                HandleTemplateAccess(template, selectedUserIds); 
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
                if (!string.IsNullOrEmpty(template.ImagePath))
                {
                    await _cloudinary.DeleteImageAsync(template.ImagePath);
                }
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
                return View(template);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>()); 
            }

            term = term.Trim().ToLower();

            var users = await _context.Users
                .Where(u =>
                    u.UserName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term))
                .OrderBy(u => u.UserName) 
                .Take(20) 
                .Select(u => new
                {
                    value = u.Id, 
                    label = $"{u.UserName} ({u.Email})" 
                })
                .ToListAsync();

            return Json(users);
        }

        private void HandleTemplateAccess(Template template, List<string> selectedUserIds)
        {
            if (template?.Id == null || selectedUserIds == null) return;

            var existing = _context.TemplateAccess
                .Where(ta => ta.TemplateId == template.Id)
                .ToList();

            _context.TemplateAccess.RemoveRange(existing);

            foreach (var userId in selectedUserIds.Distinct())
            {
                if (_context.Users.Any(u => u.Id == userId))
                {
                    _context.TemplateAccess.Add(new TemplateAccess
                    {
                        TemplateId = template.Id,
                        UserId = userId
                    });
                }
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Template model,
            IFormFile? ImageFile,
            bool removeImage = false,
            List<string> tags = null,
            List<string> selectedUserIds = null)
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

                string oldImageUrl = existingTemplate.ImagePath;

                existingTemplate.Title = model.Title;
                existingTemplate.Description = model.Description;
                existingTemplate.TopicId = model.TopicId;
                existingTemplate.UpdatedAt = DateTime.UtcNow;

                if (removeImage && !string.IsNullOrEmpty(existingTemplate.ImagePath))
                {
                    await _cloudinary.DeleteImageAsync(existingTemplate.ImagePath);
                    existingTemplate.ImagePath = null;
                }
                else if (ImageFile != null && ImageFile.Length > 0)
                {
                    var newImageUrl = await _cloudinary.UploadImageAsync(ImageFile);
                    existingTemplate.ImagePath = newImageUrl;

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        await _cloudinary.DeleteImageAsync(oldImageUrl);
                    }
                }

                foreach (var question in model.Questions)
                {
                    if (question.Id == 0) 
                    {
                        question.TemplateId = existingTemplate.Id;
                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync(); 

                        foreach (var option in question.Options)
                        {
                            if (!string.IsNullOrWhiteSpace(option.Text))
                            {
                                option.QuestionId = question.Id;
                                _context.QuestionOptions.Add(option);
                            }
                        }
                    }
                    else 
                    {
                        var existingQuestion = existingTemplate.Questions
                            .FirstOrDefault(q => q.Id == question.Id);

                        if (existingQuestion != null)
                        {
                            existingQuestion.Text = question.Text;
                            existingQuestion.Type = question.Type;
                            existingQuestion.IsRequired = question.IsRequired;
                            existingQuestion.Order = question.Order;

                            foreach (var option in question.Options)
                            {
                                if (option.Id == 0) 
                                {
                                    if (!string.IsNullOrWhiteSpace(option.Text))
                                    {
                                        option.QuestionId = existingQuestion.Id;
                                        _context.QuestionOptions.Add(option);
                                    }
                                }
                                else 
                                {
                                    var existingOption = existingQuestion.Options
                                        .FirstOrDefault(o => o.Id == option.Id);

                                    if (existingOption != null)
                                    {
                                        existingOption.Text = option.Text;
                                    }
                                }
                            }

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

                var questionsToDelete = existingTemplate.Questions
                    .Where(q => !model.Questions.Any(mq => mq.Id == q.Id))
                    .ToList();

                foreach (var question in questionsToDelete)
                {
                    var options = await _context.QuestionOptions
                        .Where(o => o.QuestionId == question.Id)
                        .ToListAsync();

                    _context.QuestionOptions.RemoveRange(options);
                    _context.Questions.Remove(question);
                }
                HandleTemplateAccess(existingTemplate, selectedUserIds);

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
    
