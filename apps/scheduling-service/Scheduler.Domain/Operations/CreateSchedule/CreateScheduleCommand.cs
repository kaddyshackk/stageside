namespace StageSide.Scheduler.Domain.Operations.CreateSchedule
{
    public record CreateScheduleCommand
    {
        public required Guid SourceId { get; init; }
        public required Guid SkuId { get; init; }
        public required string Name { get; init; }
        public string? CronExpression { get; init; }
    }
}