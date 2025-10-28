using ComedyPull.Domain.Interfaces.Service;
using Microsoft.Extensions.Hosting;

namespace ComedyPull.Application.Services
{
    public class SchedulingService(IQueueClient queueClient) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // var schedule = null;
                // if (schedule != null)
                // {
                //     var urls = await GetUrlsFromScheduleAsync(stoppingToken);
                //     var batch = await batchRepository.CreateBatch(schedule.Source, schedule.Sku, "System", stoppingToken);
                //     
                //     // TODO: Introduce dedicated mapper
                //     var requests = urls.Select(u => new CollectionRequest
                //     {
                //         BatchId = batch.Id,
                //         Url = u,
                //         Source = schedule.Source,
                //         Sku = schedule.Sku,
                //         Type = schedule.Type
                //     }).ToList();
                //
                //     await queueClient.EnqueueBatchAsync(Queues.Collection, requests);
                // }
                
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).WaitAsync(stoppingToken);
            }
        }

        private Task<string[]> GetUrlsFromScheduleAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}