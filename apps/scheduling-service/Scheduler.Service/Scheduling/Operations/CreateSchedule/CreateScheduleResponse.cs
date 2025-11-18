namespace StageSide.Scheduler.Service.Scheduling.Operations.CreateSchedule
{
    public class CreateScheduleResponse
    {
        public required string Id { get; set; }
        public required string SkuId { get; set; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public DateTimeOffset? LastExecuted { get; set; }
        public DateTimeOffset? NextExecution { get; set; }
    }
}