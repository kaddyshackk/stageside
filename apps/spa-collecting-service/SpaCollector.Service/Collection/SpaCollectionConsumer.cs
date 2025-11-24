using Coravel.Queuing.Interfaces;
using MassTransit;
using StageSide.Contracts.Scheduling.Commands;
using StageSide.SpaCollector.Domain.Collection;

namespace StageSide.SpaCollector.Service.Collection;

public class SpaCollectionConsumer(IQueue queue): IConsumer<StartSpaCollectionJobCommand>
{
    public Task Consume(ConsumeContext<StartSpaCollectionJobCommand> context)
    {
        queue.QueueInvocableWithPayload<SpaCollectionJob, StartSpaCollectionJobCommand>(context.Message);
        return Task.CompletedTask;
    }
}