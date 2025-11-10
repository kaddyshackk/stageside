using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Queue.Models;
using ComedyPull.Domain.Scheduling;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Service.Pipeline.Dispatching
{
    public class DispatchingService(
        IServiceScopeFactory scopeFactory,
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IOptions<DispatchingOptions> options,
        ILogger<DispatchingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            using (LogContext.PushProperty("ServiceName", nameof(DispatchingService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(DispatchingService));
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Collection))
                        {
                            logger.LogWarning("Collection queue overloaded, delaying for another interval.");
                            continue;
                        }

                        using var scope = scopeFactory.CreateScope();
                        var jobService = scope.ServiceProvider.GetRequiredService<SchedulingService>();
                
                        var entries = await jobService.ScheduleNextJobAsync(ct);
                        await queueClient.EnqueueBatchAsync(Queues.Collection, entries);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in dispatching service execution");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), ct);
                    }
                }
                logger.LogInformation("Stopped {ServiceName}", nameof(DispatchingService));
            }
        }
    }
}