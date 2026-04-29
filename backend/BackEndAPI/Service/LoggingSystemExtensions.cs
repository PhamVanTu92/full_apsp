namespace BackEndAPI.Service;

public sealed class SystemLogWriteRequest
{
    public string Level { get; init; } = "INFO";
    public string Action { get; init; } = "Custom";
    public string Message { get; init; } = string.Empty;
    public string? ObjType { get; init; }
    public int? ObjId { get; init; }
    public bool SaveChangesImmediately { get; init; }
}

public static class LoggingSystemServiceExtensions
{
    public static Task WriteAsync(this LoggingSystemService loggingSystemService, SystemLogWriteRequest request)
    {
        ArgumentNullException.ThrowIfNull(loggingSystemService);
        ArgumentNullException.ThrowIfNull(request);

        var level = string.IsNullOrWhiteSpace(request.Level) ? "INFO" : request.Level.Trim().ToUpperInvariant();
        var action = string.IsNullOrWhiteSpace(request.Action) ? "Custom" : request.Action.Trim();
        var message = request.Message?.Trim() ?? string.Empty;

        return request.SaveChangesImmediately
            ? loggingSystemService.SaveWithTransAsync(level, action, message, request.ObjType, request.ObjId)
            : loggingSystemService.SaveAsync(level, action, message, request.ObjType, request.ObjId);
    }

    public static Task InfoAsync(
        this LoggingSystemService loggingSystemService,
        string action,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.WriteAsync(new SystemLogWriteRequest
    {
        Level = "INFO",
        Action = action,
        Message = message,
        ObjType = objType,
        ObjId = objId,
        SaveChangesImmediately = saveChangesImmediately
    });

    public static Task WarnAsync(
        this LoggingSystemService loggingSystemService,
        string action,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.WriteAsync(new SystemLogWriteRequest
    {
        Level = "WARN",
        Action = action,
        Message = message,
        ObjType = objType,
        ObjId = objId,
        SaveChangesImmediately = saveChangesImmediately
    });

    public static Task ErrorAsync(
        this LoggingSystemService loggingSystemService,
        string action,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.WriteAsync(new SystemLogWriteRequest
    {
        Level = "ERROR",
        Action = action,
        Message = message,
        ObjType = objType,
        ObjId = objId,
        SaveChangesImmediately = saveChangesImmediately
    });

    public static Task CreateAsync(
        this LoggingSystemService loggingSystemService,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.InfoAsync("Create", message, objType, objId, saveChangesImmediately);

    public static Task UpdateAsync(
        this LoggingSystemService loggingSystemService,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.InfoAsync("Update", message, objType, objId, saveChangesImmediately);

    public static Task DeleteAsync(
        this LoggingSystemService loggingSystemService,
        string message,
        string? objType = null,
        int? objId = null,
        bool saveChangesImmediately = false
    ) => loggingSystemService.InfoAsync("Delete", message, objType, objId, saveChangesImmediately);
}
