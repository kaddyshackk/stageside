using System.Text.Json;
using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Services
{
    public class TransformationService(
        IQueueClient queueClient,
        ITransformerFactory transformerFactory,
        ILogger<ProcessingService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(TransformationService));
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await queueClient.DequeueBatchAsync(Queues.Transformation, 20);
                
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
            logger.LogInformation("Stopping {Service}", nameof(TransformationService));
        }
    }
}