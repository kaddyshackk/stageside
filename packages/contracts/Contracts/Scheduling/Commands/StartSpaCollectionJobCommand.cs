namespace StageSide.Contracts.Scheduling.Commands;

public class StartSpaCollectionJobCommand
{
    public required Guid JobId { get; init; }
    public required Guid SkuId { get; init; }
    public required string SkuName { get; init; }
}