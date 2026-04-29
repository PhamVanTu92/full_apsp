namespace BackEndAPI.Service.Notification;

public class NotificationInitializer(IServiceProvider serviceProvider): IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var approval = scope.ServiceProvider.GetRequiredService<Service.Notification.Notification>();
        return Task.CompletedTask;
    }

   public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}