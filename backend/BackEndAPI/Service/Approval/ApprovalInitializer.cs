namespace BackEndAPI.Service.Approval;

public class ApprovalInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var approval = scope.ServiceProvider.GetRequiredService<Approval>();
        // Khởi tạo Approval để đăng ký sự kiện
        return Task.CompletedTask;
    }

   public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}