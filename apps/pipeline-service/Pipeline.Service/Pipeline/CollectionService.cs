using System.Text.Json;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Domain.WebBrowser.Options;
using StageSide.Pipeline.Service.Pipeline.Options;

namespace StageSide.Pipeline.Service.Pipeline
{
    public class CollectionService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IPipelineAdapterFactory pipelineAdapterFactory,
        IOptions<CollectionOptions> options,
        IOptions<WebBrowserResourceOptions> resourceOptions,
        ILogger<CollectionService> logger
        ) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workers = Enumerable.Range(0, resourceOptions.Value.BrowserConcurrency)
                .Select(_ => WorkerAsync(stoppingToken))
                .ToArray();
            await Task.WhenAll(workers);
        }

        private async Task WorkerAsync(CancellationToken ct)
        {
            using (LogContext.PushProperty("ServiceName", nameof(CollectionService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(CollectionService));
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Transformation))
                        {
                            logger.LogWarning("Transformation queue overloaded, delaying for another interval.");
                            continue;
                        }
                        if (await queueClient.GetLengthAsync(Queues.Collection) == 0) continue;

                        var context = await queueClient.DequeueAsync(Queues.Collection);
                        if (context == null) continue;
                        
                        using (LogContext.PushProperty("ContextId", context.Id))
                        using (LogContext.PushProperty("ExecutionId", context.JobId))
                        using (LogContext.PushProperty("Sku", context.Sku))
                        using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
                        using (LogContext.PushProperty("Tags", context.Metadata.Tags))
                        {
                            var adapter = pipelineAdapterFactory.GetAdapter(context.Sku);
                            var collector = adapter.GetCollector();
                            var result = await collector.CollectAsync(context.Metadata.CollectionUrl, ct);

                            context.RawData = JsonSerializer.Serialize(result);
                            context.Metadata.CollectedAt = DateTimeOffset.UtcNow;
                            context.State = ProcessingState.Collected;

                            await queueClient.EnqueueAsync(Queues.Transformation, context);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in collection worker");
                    }
                    finally
                    {
                        var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Transformation,
                            options.Value.DelayIntervalSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
                    }
                }
                logger.LogInformation("Stopped {ServiceName}", nameof(CollectionService));
            }
        }
    }
}