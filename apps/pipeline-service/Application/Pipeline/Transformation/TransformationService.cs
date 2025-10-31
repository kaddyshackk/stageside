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
        IQueueHealthMonitor queueHealthMonitor,
        IBackPressureManager backPressureManager,
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
                try
                {
                    if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Processing))
                    {
                        logger.LogWarning("Processing queue overloaded. Applying back pressure delay.");
                        continue;
                    }

                    var adaptiveBatchSize = await backPressureManager.CalculateAdaptiveBatchSizeAsync(
                        Queues.Transformation,
                        options.Value.MinBatchSize,
                        options.Value.MaxBatchSize);
                    
                    var transformationStartTime = DateTime.UtcNow;

                    var batch = await queueClient.DequeueBatchAsync(
                        Queues.Transformation,
                        maxCount: adaptiveBatchSize,
                        stoppingToken: stoppingToken);
                    if (batch.Count <= 0) continue;
                    
                    logger.LogInformation("Transforming batch of {BatchSize} items (adaptive size: {AdaptiveSize})",
                        batch.Count, adaptiveBatchSize);

                    await TransformBatchAsync(batch, stoppingToken);

                    var transformationTime = DateTime.UtcNow - transformationStartTime;
                    await queueHealthMonitor.RecordDequeueAsync(Queues.Transformation, batch.Count,
                        transformationTime);

                    var successfulItems = batch.Where(c => c.State == ProcessingState.Transformed).ToList();
                    if (successfulItems.Count <= 0) continue;
                    
                    await queueClient.EnqueueBatchAsync(Queues.Processing, successfulItems);
                    await queueHealthMonitor.RecordEnqueueAsync(Queues.Processing, successfulItems.Count);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in transformation service execution");
                    await queueHealthMonitor.RecordErrorAsync(Queues.Transformation);
                }
                finally
                {
                    var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Processing,
                        options.Value.DelayIntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }
            }
            logger.LogInformation("Stopping {Service}", nameof(TransformationService));
        }


        private async Task TransformBatchAsync(ICollection<PipelineContext> batch, CancellationToken stoppingToken)
        {
            foreach (var context in batch)
            {
                var transformer = transformerFactory.GetTransformer(context.Sku);
                if (transformer == null)
                {
                    logger.LogWarning("Found no transformer that matched content sku {Sku}", context.Sku);
                    context.State = ProcessingState.Failed;
                    continue;
                }

                try
                {
                    context.ProcessedEntities = transformer.Transform(context.RawData);
                    context.State = ProcessingState.Transformed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to transform context {ContextId} with sku {Sku}",
                        context.Id, context.Sku);
                    context.State = ProcessingState.Failed;
                    await queueHealthMonitor.RecordErrorAsync(Queues.Transformation);
                }
            }
        }
    }
}