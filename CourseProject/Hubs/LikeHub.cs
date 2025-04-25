using CourseProject.DataUser;
using CourseProject.Models.MainModelViews.HelpModel;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class LikeHub : Hub
{
    private readonly AppUserDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public LikeHub(AppUserDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task ToggleLike(int templateId)
    {
        var user = await _userManager.GetUserAsync(Context.User);
        if (user == null) return;

        var template = await _context.Templates
            .Include(t => t.Likes)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null) return;

        var existingLike = template.Likes
            .FirstOrDefault(l => l.UserId == user.Id);

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            template.Likes.Add(new Like
            {
                UserId = user.Id,
                LikedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        var likesCount = await _context.Likes
            .CountAsync(l => l.TemplateId == templateId);

        await Clients.All.SendAsync("ReceiveLikeUpdate", templateId, likesCount);
    }
}