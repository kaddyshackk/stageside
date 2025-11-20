using Coravel.Invocable;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StageSide.Contracts.Scheduling.Commands;
using StageSide.Pipeline.Interfaces;
using StageSide.SpaCollector.Domain.Collection.Interfaces;
using StageSide.SpaCollector.Domain.Database;

namespace StageSide.SpaCollector.Domain.Collection;

public class SpaCollectionJob(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime applicationLifetime) : IInvocable, IInvocableWithPayload<StartSpaCollectionJobCommand>
{
    public StartSpaCollectionJobCommand Payload { get; set; }
    
    public async Task Invoke()
    {
        // TODO: Add operations to record job status in microservice db
        
        var ct = applicationLifetime.ApplicationStopping;
        await using var scope = scopeFactory.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<ISpaCollectingDbContextSession>();
        var sitemapLoader = scope.ServiceProvider.GetRequiredService<ISitemapLoader>();
        var pipelineAdapterFactory = scope.ServiceProvider.GetRequiredService<IPipelineAdapterFactory>();
        
        var config = await session.CollectionConfigs.Query()
            .Include(x => x.Sitemaps)
            .Where(x => x.SkuId == Payload.SkuId)
            .FirstOrDefaultAsync(ct);
        if (config is null)
        {
            throw new ArgumentException($"Collection configuration with sku id {Payload.SkuId} not found");
        }
        
        var sitemaps = config.Sitemaps.Where(x => x.IsActive).ToList();
        if (sitemaps.Count == 0)
        {
            throw new ArgumentException($"Collection configuration with sku id {Payload.SkuId} is missing sitemaps.");
        }

        var urls = await sitemapLoader.LoadManySitemapsAsync(sitemaps);

        var pipelineAdapter = pipelineAdapterFactory.GetAdapter(Payload.SkuName);
        await Parallel.ForEachAsync(
            urls,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = config.MaxConcurrency,
                CancellationToken = ct
            },
            async (url, ct) =>
            {
                var collector = pipelineAdapter.GetCollector();
                var result = await collector.CollectAsync(url, ct);

                // TODO: Write result to S3
            });
        
        // TODO: Invoke event that job was completed
    }
}