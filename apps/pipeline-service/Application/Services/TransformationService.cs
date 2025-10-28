using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                var transformed = new List<SilverRecord>();
                var batch = await queueClient.DequeueBatchAsync(Queues.Transformation, 20);
                
                foreach (var context in batch)
                {
                    var transformer = transformerFactory.GetTransformer(context.Sku);
                    if (transformer == null)
                    {
                        logger.LogWarning("Found no transformer that matched content sku {Sku}", context.Sku);
                        continue;
                    }

                    transformed.AddRange(transformer.Transform(context));
                }

                await queueClient.EnqueueBatchAsync(Queues.Processing, transformed);
            }
        }
    }
}