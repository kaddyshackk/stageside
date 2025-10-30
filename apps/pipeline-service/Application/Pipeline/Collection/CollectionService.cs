using ComedyPull.Domain.Configuration;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Pipeline.Collection
{
    /// <summary>
    /// This service is responsible for pulling resource requests from the queue and delegating to collectors.
    /// </summary>
    public class CollectionService(
        IQueueClient queueClient,
        IOptions<CollectionOptions> options,
        ILogger<CollectionService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(CollectionService));
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await queueClient.GetLengthAsync(Queues.Collection) > 0)
                {
                    var context = await queueClient.DequeueAsync(Queues.Collection);
                    if (context == null)
                    {
                        continue;
                    }

                    switch (SkuConfiguration.GetCollectionType(context.Sku))
                    {
                        case CollectionType.Dynamic:
                            await queueClient.EnqueueAsync(Queues.DynamicCollection, context);
                            break;
                        case CollectionType.Static:
                            throw new NotImplementedException("Static collector not implemented yet");
                        case CollectionType.Api:
                            throw new NotImplementedException("API collector not implemented yet");
                        default:
                            throw new NotSupportedException($"Collection type {context} not supported");
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
                }
            }
            logger.LogInformation("Stopping {Service}", nameof(CollectionService));
        }
    }
}