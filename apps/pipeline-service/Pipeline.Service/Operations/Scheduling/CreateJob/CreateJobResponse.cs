namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateJob
{
    public class CreateJobResponse
    {
        public required string Id { get; set; }
        public required string Source { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public string? CronExpression { get; set; }
        public required bool IsActive { get; set; }
        public DateTimeOffset? LastExecuted { get; set; }
        public DateTimeOffset? NextExecution { get; set; }
    }
}