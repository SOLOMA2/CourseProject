using CourseProject.DataUser;
using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

public class CommentHub : Hub
{
    private readonly AppUserDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CommentHub(AppUserDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SendComment(int templateId, string content)
    {
        var user = await _userManager.GetUserAsync(Context.User);

        // Сохраняем в базу
        var comment = new Comment
        {
            Content = content,
            TemplateId = templateId,
            AuthorId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Отправляем всем клиентам
        await Clients.All.SendAsync("ReceiveComment", new
        {
            content = comment.Content,
            authorName = user.UserName,
            date = comment.CreatedAt
        });
    }
}