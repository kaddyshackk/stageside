using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Services
{
    /// <summary>
    /// This service is responsible for pulling resource requests from the queue and delegating to collectors.
    /// </summary>
    public class CollectionService(
        IQueueClient queueClient,
        ILogger<CollectionService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting CollectionService");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await queueClient.GetLengthAsync(Queues.Collection) > 0)
                {
                    var request = await queueClient.DequeueAsync(Queues.Collection);
                    if (request == null)
                    {
                        continue;
                    }

                    switch (request.Type)
                    {
                        case CollectionType.Dynamic:
                            await queueClient.EnqueueAsync(Queues.DynamicCollection, request);
                            break;
                        case CollectionType.Static:
                            throw new NotImplementedException("Static collector not implemented yet");
                        case CollectionType.Api:
                            throw new NotImplementedException("API collector not implemented yet");
                        default:
                            throw new NotSupportedException($"Collection type {request} not supported");
                    }
                    
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            logger.LogInformation("CollectionService stopped");
        }
    }
}