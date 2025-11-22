namespace StageSide.Scheduler.Domain.Operations.CreateSource
{
    public record CreateSourceCommand
    {
        public required string Name { get; init; }
        public required string Website { get; init; }
    }
}