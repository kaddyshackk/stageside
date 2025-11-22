using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.Scheduler.Domain.Database;
using StageSide.Scheduler.Domain.Models;
using StageSide.Scheduler.Domain.Operations.CreateSchedule;
using StageSide.Scheduler.Domain.Operations.CreateSku;
using StageSide.Scheduler.Domain.Operations.CreateSource;
using StageSide.Scheduler.Domain.Operations.GetSource;

namespace StageSide.Scheduler.Domain.Scheduling
{
    public class SchedulingService(ISchedulingDbContextSession session, ILogger<SchedulingService> logger)
    {
        public async Task<Source?> GetSourceByIdAsync(GetSourceQuery query, CancellationToken ct)
        {
            return await session.Sources.Query()
                .AsNoTracking()
                .Include(x => x.Skus)
                .FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        }
        
        public async Task<Source?> CreateSourceAsync(CreateSourceCommand command, CancellationToken ct)
        {
            try
            {
                var existing = await session.Sources.Query()
                    .AsNoTracking()
                    .Where(s => s.Name == command.Name || s.Website == command.Website)
                    .FirstOrDefaultAsync(ct);

                if (existing != null) throw new ArgumentException("Source already exists.");

                var source = new Source
                {
                    Name = command.Name,
                    Website = command.Website,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = "System"
                };
                
                await session.Sources.AddAsync(source, ct);
                await session.SaveChangesAsync(ct);
                
                return source;
            }
            catch (Exception ex)
            {
                session.Dispose();
                logger.LogError(ex, "Failed to create source.");
                throw;
            }
        }

        public async Task<Sku?> CreateSkuAsync(CreateSkuCommand command, CancellationToken ct)
        {
            try
            {
                var existing = await session.Skus.Query()
                    .AsNoTracking()
                    .Where(x => x.Name == command.Name && x.SourceId == command.SourceId)
                    .FirstOrDefaultAsync(ct);
                if (existing != null) throw new ArgumentException("Sku already exists.");

                var sku = new Sku
                {
                    SourceId = command.SourceId,
                    Name = command.Name,
                    Type = command.Type,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = "System"
                };
                await session.Skus.AddAsync(sku, ct);
                await session.SaveChangesAsync(ct);

                return sku;
            }
            catch (Exception ex)
            {
                session.Dispose();
                logger.LogError(ex, "Failed to create sku.");
                throw;
            }
        }
        
        public async Task<Schedule?> CreateScheduleAsync(CreateScheduleCommand command, CancellationToken ct)
        {
            try
            {
                var nextOccurence = string.IsNullOrEmpty(command.CronExpression)
                    ? DateTimeOffset.UtcNow
                    : CronCalculationService.CalculateNextOccurence(command.CronExpression);
                if (nextOccurence == null)
                {
                    throw new ArgumentException("Failed to determine next occurence for new schedule.");
                }
                
                var sku = await session.Skus.GetByIdAsync(command.SkuId, ct);
                if (sku == null)
                {
                    throw new ArgumentException($"Sku could not be found matching SkuId {command.SkuId}");
                }

                var now = DateTimeOffset.UtcNow;
                var schedule = new Schedule
                {
                    SourceId = command.SourceId,
                    SkuId = sku.Id,
                    Name = command.Name,
                    CronExpression = command.CronExpression,
                    IsActive = true,
                    NextExecution = nextOccurence.Value,
                    CreatedAt = now,
                    CreatedBy = "System",
                    UpdatedAt = now,
                    UpdatedBy = "System",
                };
                await session.Schedules.AddAsync(schedule, ct);
                await session.SaveChangesAsync(ct);

                return await session.Schedules.GetByIdAsync(schedule.Id, ct);
            }
            catch (Exception ex)
            {
                session.Dispose();
                logger.LogError(ex, "Failed to create schedule.");
                throw;
            }
        }
    }
}