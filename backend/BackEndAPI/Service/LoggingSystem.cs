using BackEndAPI.Data;
using BackEndAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service;

public class LoggingSystemService(
    AppDbContext dbContext,
    IHttpContextAccessor contextAccessor,
    UserContextService contextService 
)
{
    public async Task SaveWithTransAsync(string level, string action, string message, string? objType = null, int? objId = null)
    {
        await SaveAsync(level, action, message, objType, objId);
        await dbContext.SaveChangesAsync();
    }
    public async Task SaveAsync(string level ,string action, string message, string? objType = null, int? objId = null)
    {
        var user = dbContext.AppUser.AsNoTracking().FirstOrDefault(x => x.Id == contextService.UserId);
        // if (user == null) return;
        var context = contextAccessor.HttpContext;
        var ip = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                 context?.Connection.RemoteIpAddress?.ToString();

        var userAgent = context?.Request.Headers["User-Agent"].ToString();

        var newLog = new SystemLog
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Action = action,
            Message = message,
            ActorId = user?.Id ?? 0,
            ActorName = user?.FullName ?? "<unknow>",
            ActorUserName = user?.UserName ?? "<unknow>",
            ObjId = objId,
            ObjType = objType,
            // Logger = null,
            Exception = null,
            IpAddress = ip,
            Url = context?.Request.Path.ToString(),
            Device = userAgent,
            HttpMethod = context?.Request.Method
            // RequestBody = null,
            // ResponseBody = null
        };

        dbContext.SystemLogs.Add(newLog);
    }
}