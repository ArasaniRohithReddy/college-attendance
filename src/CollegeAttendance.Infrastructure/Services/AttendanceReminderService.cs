using CollegeAttendance.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CollegeAttendance.Infrastructure.Services;

public class AttendanceReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AttendanceReminderService> _logger;

    public AttendanceReminderService(IServiceScopeFactory scopeFactory, ILogger<AttendanceReminderService> logger)
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

                // Run every 15 minutes during working hours (7 AM - 6 PM)
                if (now.Hour >= 7 && now.Hour <= 18)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sessionService = scope.ServiceProvider.GetRequiredService<IClassSessionService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    // This is a placeholder — in production, query sessions starting in the next 15 min
                    _logger.LogInformation("Attendance reminder check completed at {Time}", now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AttendanceReminderService");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
