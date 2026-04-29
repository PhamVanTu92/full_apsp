using BackEndAPI.Data;
using BackEndAPI.Service.Promotions;
using Quartz;

namespace BackEndAPI.Service.Jobs;

/// <summary>
/// Tính điểm tự động cho các đơn hàng đã xác nhận (status = "DXN") chưa có CustomerPointLine.
/// Lưu ý: hiện tại body bị comment, chưa thực sự gọi CalculatePoints — cần làm rõ trước khi enable.
/// </summary>
public class AutoPointJob : IJob
{
    private readonly ILogger<AutoPointJob> _logger;
    private readonly AppDbContext _context;
    private readonly IPointSetupService _pointService;

    public AutoPointJob(ILogger<AutoPointJob> logger, AppDbContext context, IPointSetupService pointService)
    {
        _logger = logger;
        _context = context;
        _pointService = pointService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var orders = _context.ODOC
            .Where(o => o.Status == "DXN")
            .Where(o => !_context.CustomerPointLine.Any(h => h.DocId == o.Id))
            .ToList();

        _logger.LogInformation("[AutoPointJob] {Count} orders pending point calculation (currently disabled)", orders.Count);

        // TODO: bật khi xác nhận business logic CalculatePoints đúng
        //foreach (var order in orders)
        //    await _pointService.CalculatePoints(order.Id);

        return Task.CompletedTask;
    }
}
