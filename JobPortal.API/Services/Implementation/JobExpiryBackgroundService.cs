using JobPortal.API.Services.Interface;

namespace JobPortal.API.Services.Implementation;

public class JobExpiryBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobExpiryBackgroundService> _logger;

    public JobExpiryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<JobExpiryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CloseExpiredAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CloseExpiredAsync(stoppingToken);
        }
    }

    private async Task CloseExpiredAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var expiryService = scope.ServiceProvider.GetRequiredService<IJobExpiryService>();
            var closed = await expiryService.CloseExpiredJobsAsync(cancellationToken);
            if (closed > 0)
            {
                _logger.LogInformation("Auto-closed {Count} expired job(s).", closed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-close expired jobs.");
        }
    }
}
