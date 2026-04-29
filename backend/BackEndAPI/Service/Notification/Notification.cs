using BackEndAPI.Data;
using BackEndAPI.Models.NotificationModels;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.EventAggregator;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Notification;

public class Notification
{
    private readonly AppDbContext _context;
    private readonly IServiceScopeFactory _serviceProvider;
    private readonly WebSocketService _webSocketService;

    public Notification(AppDbContext context, IServiceScopeFactory serviceProvider,
        IEventAggregator eventAggregator, WebSocketService webSocketService)
    {
        _context = context;
        eventAggregator.Subscribe<Models.NotificationModels.Notification>(CreateNotification);
        _serviceProvider = serviceProvider;
        _webSocketService = webSocketService;
    }

    // async void được phép cho event handler (đăng ký qua IEventAggregator.Subscribe<T>(Action<T>)
    // ở constructor). Try/catch nội bộ giữ exception không thoát ra khỏi handler — bắt buộc với
    // async void để không crash process.
    private async void CreateNotification(Models.NotificationModels.Notification noti)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var trans = await context.Database.BeginTransactionAsync();
        try
        {
            var userId = noti.SendUsers.Select(p => p.ToString()).ToList();
            userId.Add("2475");
            noti.Users = new List<NotificationUser>();
            foreach (var id in userId)
            {
                var idInt = int.Parse(id);
                noti.Users.Add(new NotificationUser
                {
                    UserId = idInt,
                    IsRead = false,
                });
            }

            context.Notifications.Add(noti);
            await context.SaveChangesAsync();

            await trans.CommitAsync();
            _ = _webSocketService.SendMessageToUsers(userId, noti);
        }
        catch
        {
            await trans.RollbackAsync();
        }
    }

    public async Task<(List<NotificationUser>?, Mess?, int, int)> GetUserNotifications(int userId, int skip,
        int limit)
    {
        var mess = new Mess();
        try
        {
            var query = _context.NotificationUsers.Where(u => u.UserId  == userId).AsNoTracking().AsQueryable();
            var notiCount = await query.CountAsync();
            var readCount = await query.CountAsync(u => u.IsRead == true);

            var notiList = await query
                .Include(u => u.Notification)
                .ThenInclude(u => u.Object)
                .Where(u => u.IsHide != true)
                .OrderByDescending(u => u.Id)
                .Skip(skip * limit)
                .Take(limit)
                .ToListAsync();
            

            return (notiList, null, notiCount, readCount);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess, 0, 0);
        }
    }

    public async Task<Mess?> ChangeStatusNotification(int id,bool isRead)
    {
        var mess = new Mess();
        try
        {
            var noti = await _context.NotificationUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (noti == null)
            {
                mess.Errors = $"Notification with id {id} not found";
                mess.Status = 404;

                return mess;
            }

            if (isRead)
            {
                noti.ReadAt = DateTime.Now;
            }

            noti.IsRead = isRead;
            await _context.SaveChangesAsync();

            return null;
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return mess;
        }
    }
}