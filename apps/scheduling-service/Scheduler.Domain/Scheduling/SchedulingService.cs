using Microsoft.Extensions.Logging;
using StageSide.Scheduler.Domain.Database;
using StageSide.Scheduler.Domain.Models;
using StageSide.Scheduler.Domain.Scheduling.Operations.CreateSchedule;

namespace StageSide.Scheduler.Domain.Scheduling
{
    public class SchedulingService(ISchedulingDbContextSession session, ILogger<SchedulingService> logger)
    {
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