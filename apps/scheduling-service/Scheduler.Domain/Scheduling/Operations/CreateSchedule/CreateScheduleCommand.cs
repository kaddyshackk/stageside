namespace StageSide.Scheduler.Domain.Scheduling.Operations.CreateSchedule
{
    public record CreateScheduleCommand
    {
        public required Guid SkuId { get; init; }
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
    }
}