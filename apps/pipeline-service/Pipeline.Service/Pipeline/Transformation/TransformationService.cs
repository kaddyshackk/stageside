using System.Text.Json;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Queue.Models;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StageSide.Pipeline.Domain.PipelineAdapter;

namespace StageSide.Pipeline.Service.Pipeline.Transformation
{
    public class TransformationService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IPipelineAdapterFactory adapterFactory,
        IOptions<TransformationOptions> options,
        ILogger<TransformationService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(TransformationService)))
            {
                logger.LogInformation("Started {Pipeline.Service}", nameof(TransformationService));
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
                logger.LogInformation("Stopped {Pipeline.Service}", nameof(TransformationService));
            }
        }
        
        private void TransformBatch(ICollection<PipelineContext> batch)
        {
            foreach (var context in batch)
            {
                using (LogContext.PushProperty("ContextId", context.Id))
                using (LogContext.PushProperty("ExecutionId", context.JobId))
                using (LogContext.PushProperty("Sku", context.Sku))
                using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
                using (LogContext.PushProperty("Tags", context.Metadata.Tags))
                {
                    var adapter = adapterFactory.GetAdapter(context.Sku);
                    var transformer = adapter.GetTransformer();

                    try
                    {
                        // Deserialize RawData back to object
                        object? deserializedData = null;
                        if (context.RawData is { } jsonString && !string.IsNullOrEmpty(jsonString))
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