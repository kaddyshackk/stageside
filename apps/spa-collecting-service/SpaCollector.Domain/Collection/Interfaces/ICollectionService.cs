using StageSide.Contracts.Scheduling.Commands;

namespace StageSide.SpaCollector.Domain.Collection.Interfaces;

public interface ICollectionService
{
    public Task CollectAsync(StartSpaCollectionJobCommand jobCommand, CancellationToken ct);
}