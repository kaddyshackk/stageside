using System.ComponentModel.DataAnnotations;

namespace StageSide.Scheduler.Service.Operations.CreateSchedule
{
    public record CreateScheduleRequest
    {
        [Required]
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
    }
}