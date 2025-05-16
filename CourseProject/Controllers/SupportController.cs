using CourseProject.Models;
using CourseProject.Models.ViewsModels;
using Dropbox.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace CourseProject.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly string _dropboxToken;
        private readonly List<string> _adminEmails;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<SupportController> _logger;

        public SupportController(
            IConfiguration config,
            UserManager<AppUser> userManager,
            ILogger<SupportController> logger)
        {
            _dropboxToken = config["Dropbox:AccessToken"];
            _adminEmails = config.GetSection("Admins").Get<List<string>>();
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTicket(SupportTicket model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                model.ReportedBy = user.Email;
                model.Link = Request.Form["Link"];

                // Сериализуем модель в JSON с camelCase
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                // Загружаем в Dropbox
                using var dbx = new DropboxClient(_dropboxToken);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                var path = $"/SupportTickets/ticket_{DateTime.Now:yyyyMMddHHmmss}.json";

                await dbx.Files.UploadAsync(path, Dropbox.Api.Files.WriteMode.Overwrite.Instance, body: stream);

                TempData["SuccessMessage"] = "Круто! Ваш тикет отправлен, ожидайте ответа в течение суток.";
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error creating ticket");
                TempData["ErrorMessage"] = "Please, try again later(";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
