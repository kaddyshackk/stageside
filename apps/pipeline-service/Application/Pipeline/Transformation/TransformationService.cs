using System.Text.Json;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Pipeline.Models;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Queue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Transformation
{
    public class TransformationService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        ITransformerFactory transformerFactory,
        IOptions<TransformationOptions> options,
        ILogger<TransformationService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(TransformationService)))
            {
                logger.LogInformation("Started {Service}", nameof(TransformationService));
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

                        var batch = await queueClient.DequeueBatchAsync(
                            Queues.Transformation,
                            maxCount: adaptiveBatchSize,
                            stoppingToken: stoppingToken);
                        if (batch.Count <= 0) continue;
                        
                        logger.LogInformation("Transforming batch of {BatchSize} items (adaptive size: {AdaptiveSize})",
                            batch.Count, adaptiveBatchSize);

                        TransformBatch(batch);

                        var successfulItems = batch.Where(c => c.State == ProcessingState.Transformed).ToList();
                        if (successfulItems.Count <= 0) continue;
                        
                        await queueClient.EnqueueBatchAsync(Queues.Processing, successfulItems);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in transformation service execution");
                    }
                    finally
                    {
                        var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Processing,
                            options.Value.DelayIntervalSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {Service}", nameof(TransformationService));
            }
        }
        
        private void TransformBatch(ICollection<PipelineContext> batch)
        {
            foreach (var context in batch)
            {
                using (LogContext.PushProperty("ContextId", context.Id))
                using (LogContext.PushProperty("ExecutionId", context.JobExecutionId))
                using (LogContext.PushProperty("Sku", context.Sku))
                using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
                using (LogContext.PushProperty("Tags", context.Metadata.Tags))
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
                        // Deserialize RawData back to object
                        object? deserializedData = null;
                        if (context.RawData is string jsonString && !string.IsNullOrEmpty(jsonString))
                        {
                            deserializedData = JsonSerializer.Deserialize<JsonElement>(jsonString);
                        }
                        else
                        {
                            deserializedData = context.RawData;
                        }

                        context.ProcessedEntities = transformer.Transform(deserializedData);
                        context.State = ProcessingState.Transformed;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to transform context {ContextId} with sku {Sku}",
                            context.Id, context.Sku);
                        context.State = ProcessingState.Failed;
                    }
                }
            }
        }
    }
}