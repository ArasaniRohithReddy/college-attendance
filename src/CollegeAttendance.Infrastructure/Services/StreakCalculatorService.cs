using CollegeAttendance.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CollegeAttendance.Infrastructure.Services;

public class StreakCalculatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StreakCalculatorService> _logger;

    public StreakCalculatorService(IServiceScopeFactory scopeFactory, ILogger<StreakCalculatorService> logger)
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
                using var scope = _scopeFactory.CreateScope();
                var gamificationService = scope.ServiceProvider.GetRequiredService<IGamificationService>();

                await gamificationService.RecalculateStreaksAsync();
                await gamificationService.RecalculateLeaderboardAsync("weekly");
                await gamificationService.RecalculateLeaderboardAsync("monthly");

                _logger.LogInformation("Streak and leaderboard recalculation completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StreakCalculatorService");
            }

            // Run daily at 2 AM
            var delay = CalculateDelayUntilNextRun();
            await Task.Delay(delay, stoppingToken);
        }
    }

    private static TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1).AddHours(2); // Next day 2 AM UTC
        if (now.Hour < 2)
            nextRun = now.Date.AddHours(2);

        var delay = nextRun - now;
        return delay > TimeSpan.Zero ? delay : TimeSpan.FromHours(24);
    }
}
