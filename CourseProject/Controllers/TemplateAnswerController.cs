using CourseProject.DataUser;
using CourseProject.Models.MainModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Identity;
using CourseProject.Models;
using Microsoft.AspNetCore.SignalR;
using CourseProject.Models.MainModelViews.HelpModel;

namespace CourseProject.Controllers
{
    public class TemplateAnswerController : Controller
    {
        private readonly AppUserDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<TemplateAnswerController> _logger; 

        public TemplateAnswerController(
            AppUserDbContext context,
            UserManager<AppUser> userManager, ILogger<TemplateAnswerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> UserAnswer(int templateId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var template = await _context.Templates
                .Include(t => t.Views)
                .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) return NotFound();

            var response = new FormResponse
            {
                TemplateId = templateId,
                UserId = user.Id,
                Template = template,
                Answers = new List<Answer>()
            };

            var view = new View
            {
                TemplateId = templateId,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ViewedAt = DateTime.UtcNow
            };

            _context.Views.Add(view);
            await _context.SaveChangesAsync();

            ViewBag.Template = template;

            var updatedViewsCount = await _context.Views
                .CountAsync(v => v.TemplateId == templateId);

            var viewHub = HttpContext.RequestServices.GetRequiredService<IHubContext<ViewHub>>();
            await viewHub.Clients.All.SendAsync("ReceiveViewUpdate", templateId, updatedViewsCount);
            return View(response);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAnswer(FormResponse formResponse)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var template = await _context.Templates
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(t => t.Id == formResponse.TemplateId);

                if (template == null) return NotFound();

                formResponse.Answers = new List<Answer>();

                foreach (var question in template.Questions)
                {
                    var answer = new Answer
                    {
                        QuestionId = question.Id,
                        UserResponse = formResponse
                    };

                    switch (question.Type)
                    {
                        case QuestionType.Radio:
                        case QuestionType.Dropdown:
                            var selectedOptionId = Request.Form[$"Answers[{question.Id}].SelectedOptions"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(selectedOptionId))
                            {
                                if (int.TryParse(selectedOptionId, out int optionId))
                                {
                                    answer.SelectedOptions.Add(new SelectedOption { QuestionOptionId = optionId });
                                }
                            }
                            break;

                        case QuestionType.Checkbox:
                            var selectedOptionIds = Request.Form[$"Answers[{question.Id}].SelectedOptions"];
                            foreach (var id in selectedOptionIds)
                            {
                                if (int.TryParse(id, out int checkboxId))
                                {
                                    answer.SelectedOptions.Add(new SelectedOption { QuestionOptionId = checkboxId });
                                }
                            }
                            break;

                        case QuestionType.YesNo:
                            answer.Text = Request.Form[$"Answers[{question.Id}].Text"];
                            break;

                        default:
                            answer.Text = Request.Form[$"Answers[{question.Id}].Text"];
                            break;
                    }

                    formResponse.Answers.Add(answer);
                }

                formResponse.UserId = user.Id;
                formResponse.CreatedAt = DateTime.UtcNow;

                _context.FormResponses.Add(formResponse);
                await _context.SaveChangesAsync();

                bool isCommentSaved = false;
                var commentContent = Request.Form["comment"];

                if (!string.IsNullOrWhiteSpace(commentContent))
                {
                    var comment = new Comment
                    {
                        Content = commentContent,
                        TemplateId = formResponse.TemplateId,
                        AuthorId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Comments.Add(comment);
                    await _context.SaveChangesAsync();

                    isCommentSaved = true;

                    var hubContext = HttpContext.RequestServices
                        .GetRequiredService<IHubContext<CommentHub>>();

                    await hubContext.Clients.All.SendAsync("ReceiveNewComment",
                        comment.TemplateId,
                        new
                        {
                            content = comment.Content,
                            authorName = user.UserName,
                            date = comment.CreatedAt
                        });
                }

                TempData["IsCommentSaved"] = isCommentSaved;
                TempData["IsAnswersSaved"] = true;

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения: {Message}", ex.Message);
                ModelState.AddModelError("", "Ошибка сохранения: " + ex.Message);
                return View(formResponse);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int templateId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var comment = new Comment
            {
                Content = content,
                TemplateId = templateId,
                AuthorId = user.Id, 
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            comment.Author = user;

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<CommentHub>>();
            await hubContext.Clients.All.SendAsync("ReceiveNewComment", templateId, comment);

            return Ok();
        }

        public async Task<IActionResult> Success()
        {
            ViewBag.IsCommentSaved = TempData["IsCommentSaved"] as bool? ?? false;
            ViewBag.IsAnswersSaved = TempData["IsAnswersSaved"] as bool? ?? false;
            return View();
        }
    }
}
