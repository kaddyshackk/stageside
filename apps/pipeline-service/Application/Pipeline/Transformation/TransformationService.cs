using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Pipeline.Transformation
{
    public class TransformationService(
        IQueueClient queueClient,
        ITransformerFactory transformerFactory,
        IOptions<TransformationOptions> options,
        ILogger<TransformationService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(TransformationService));
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await queueClient.DequeueBatchAsync(
                    Queues.Transformation,
                    maxCount: options.Value.BatchMaxSize,
                    maxWait: TimeSpan.FromSeconds(options.Value.BatchMaxWaitSeconds),
                    pollingWait: TimeSpan.FromSeconds(options.Value.PollingWaitSeconds),
                    cancellationToken: stoppingToken);

                if (batch.Count > 0)
                {
                    foreach (var context in batch)
                    {
                        var transformer = transformerFactory.GetTransformer(context.Sku);
                        if (transformer == null)
                        {
                            logger.LogWarning("Found no transformer that matched content sku {Sku}", context.Sku);
                            continue;
                        }

                        try
                        {
                            context.ProcessedEntities = transformer.Transform(context.RawData);
                            context.State = ProcessingState.Transformed;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to deserialize PunchupRecord from context {ContextId}",
                                context.Id);
                            context.State = ProcessingState.Failed;
                        }
                    }

                    await queueClient.EnqueueBatchAsync(Queues.Processing, batch);
                }

                await Task.Delay(TimeSpan.FromSeconds(options.Value.BatchDelaySeconds), stoppingToken);
            }
            logger.LogInformation("Stopping {Service}", nameof(TransformationService));
        }
    }
}