using ComedyPull.Domain.Jobs.Services;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Scheduling
{
    public class SchedulingService(
        IServiceScopeFactory scopeFactory,
        IBackPressureManager backPressureManager,
        IOptions<SchedulingOptions> options,
        ILogger<SchedulingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(SchedulingService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(SchedulingService));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Collection))
                        {
                            logger.LogWarning("Collection queue overloaded, delaying for another interval.");
                            continue;
                        }

                        using var scope = scopeFactory.CreateScope();
                        var jobDispatchService = scope.ServiceProvider.GetRequiredService<DispatchService>();
                        await jobDispatchService.DispatchNextJobAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in scheduling service execution");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {ServiceName}", nameof(SchedulingService));
            }
        }
    }
}