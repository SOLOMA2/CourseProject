using CourseProject.DataUser;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ViewHub : Hub
{
    private readonly AppUserDbContext _context;

    public ViewHub(AppUserDbContext context)
    {
        _context = context;
    }

    public async Task NotifyViewUpdate(int templateId, int viewsCount)
    {
        await Clients.All.SendAsync("ReceiveViewUpdate", templateId, viewsCount);
    }
}