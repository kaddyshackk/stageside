using ComedyPull.Domain.Models.Processing;
using MediatR;

namespace ComedyPull.Application.Modules.DataProcessing.Events
{
    public record StateCompletedEvent(Guid BatchId, ProcessingState CompletedState) : INotification;
}