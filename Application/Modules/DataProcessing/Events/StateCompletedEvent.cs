using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;

namespace ComedyPull.Application.Modules.DataProcessing.Events
{
    public record StateCompletedEvent(Guid BatchId, ProcessingState CompletedState) : INotification;
}