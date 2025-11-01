using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Queue;
using ComedyPull.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Scheduling
{
    public class SchedulingService(
        JobService jobService,
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

                        await jobService.DispatchNextJobAsync(stoppingToken);
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