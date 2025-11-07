using System.Text.RegularExpressions;
using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Extensions;
using ComedyPull.Domain.Jobs.Interfaces;
using ComedyPull.Domain.Jobs.Operations.CreateJob;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Domain.Jobs.Services
{
    public class JobService(IServiceScopeFactory scopeFactory)
    {
        public async Task<Job?> GetNextJob(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            return await jobRepository.ReadNextJobAsync(stoppingToken);
        }
        
        public async Task<Job> CreateJobAsync(CreateJobCommand command, CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var sitemapRepository = scope.ServiceProvider.GetRequiredService<IJobSitemapRepository>();
            
            var nextOccurence = string.IsNullOrEmpty(command.CronExpression)
                ? DateTimeOffset.UtcNow
                : CronCalculationService.CalculateNextOccurence(command.CronExpression);
            if (nextOccurence == null)
            {
                throw new ArgumentException("Failed to determine next occurence for new job.");
            }

            var source = EnumExtensions.ParseFromDescriptionOrThrow<Source>(command.Source);
            var sku = EnumExtensions.ParseFromDescriptionOrThrow<Sku>(command.Sku);

            var job = await jobRepository.CreateJobAsync(new Job
            {
                Source = source,
                Sku = sku,
                Name = command.Name,
                CronExpression = command.CronExpression,
                IsActive = true,
                NextExecution = nextOccurence.Value,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System",
            }, stoppingToken);

            var sitemaps = new List<Sitemap>();
            
            if (command.Sitemaps?.Count > 0)
            {
                foreach (var sitemap in command.Sitemaps)
                {
                    var createdSitemap = await sitemapRepository.CreateJobSitemapAsync(new Sitemap
                    {
                        JobId = job.Id,
                        Url = sitemap.Url,
                        RegexFilter = sitemap.RegexFilter,
                    }, stoppingToken);
                    sitemaps.Add(createdSitemap);
                }
            }
            
            job.Sitemaps = sitemaps;

            return job;
        }
    }
}