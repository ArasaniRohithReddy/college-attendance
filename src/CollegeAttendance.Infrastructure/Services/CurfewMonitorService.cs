using CollegeAttendance.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CollegeAttendance.Infrastructure.Services;

public class CurfewMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurfewMonitorService> _logger;

    public CurfewMonitorService(IServiceScopeFactory scopeFactory, ILogger<CurfewMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Check curfew violations around 10 PM, 11 PM, and midnight
                if (now.Hour >= 22 || now.Hour <= 1)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var curfewService = scope.ServiceProvider.GetRequiredService<ICurfewService>();

                    await curfewService.CheckCurfewViolationsAsync();
                    _logger.LogInformation("Curfew violation check completed at {Time}", now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CurfewMonitorService");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
