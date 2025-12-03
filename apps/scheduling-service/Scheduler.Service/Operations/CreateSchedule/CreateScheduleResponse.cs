namespace StageSide.Scheduler.Service.Operations.CreateSchedule
{
    public class CreateScheduleResponse
    {
        public required string Id { get; set; }
        public required string SkuId { get; set; }
        public required string SourceId { get; set; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public DateTimeOffset? LastExecution { get; set; }
        public DateTimeOffset? NextExecution { get; set; }
    }
}
