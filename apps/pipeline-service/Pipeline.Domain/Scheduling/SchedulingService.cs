using StageSide.Pipeline.Domain.Extensions;
using StageSide.Pipeline.Domain.Operations;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.Domain.Models;
using StageSide.Scheduling.Models;

namespace StageSide.Pipeline.Domain.Scheduling
{
    public class SchedulingService(ISchedulingContextSession session, ILogger<SchedulingService> logger)
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

                var source = EnumExtensions.ParseFromDescriptionOrThrow<Source>(command.Source);
                var sku = EnumExtensions.ParseFromDescriptionOrThrow<Sku>(command.Sku);

                await session.BeginTransactionAsync(ct);

                var now = DateTimeOffset.UtcNow;
                var schedule = new Schedule
                {
                    Source = source,
                    Sku = sku,
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

                if (command.Sitemaps is { Count: > 0 })
                {
                    var sitemaps = command.Sitemaps
                        .Select(s => new Sitemap
                        {
                            ScheduleId = schedule.Id,
                            Url = s.Url,
                            RegexFilter = s.RegexFilter,
                            CreatedAt = now,
                            CreatedBy = "System",
                            UpdatedAt = now,
                            UpdatedBy = "System",
                        })
                        .ToList();
                    await session.Sitemaps.AddRangeAsync(sitemaps, ct);
                }
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);

                return await session.Schedules.Query()
                    .Include(s => s.Sitemaps)
                    .FirstOrDefaultAsync(s => s.Id == schedule.Id, ct);
            }
            catch (Exception ex)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(ex, "Failed to create schedule.");
                throw;
            }
        }
    }
}