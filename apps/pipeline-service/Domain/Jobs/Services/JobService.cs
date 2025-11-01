using ComedyPull.Domain.Extensions;
using ComedyPull.Domain.Jobs.Interfaces;
using ComedyPull.Domain.Jobs.Operations;
using ComedyPull.Domain.Models;
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
            
            var nextOccurence = string.IsNullOrEmpty(command.CronExpression)
                ? DateTimeOffset.UtcNow
                : CronCalculationService.CalculateNextOccurence(command.CronExpression);
            if (nextOccurence == null)
            {
                throw new ArgumentException("Failed to determine next occurence for new job.");
            }

            var source = EnumExtensions.ParseFromDescriptionOrThrow<Source>(command.Source);
            var sku = EnumExtensions.ParseFromDescriptionOrThrow<Sku>(command.Sku);

            return await jobRepository.CreateJobAsync(new Job
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
        }
    }
}