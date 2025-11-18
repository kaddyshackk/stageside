using System.ComponentModel.DataAnnotations;

namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
{
    public record CreateScheduleRequest
    {
        [Required]
        public required string SkuId { get; init; }
        [Required]
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
    }
}