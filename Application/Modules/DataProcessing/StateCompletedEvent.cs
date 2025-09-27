using ComedyPull.Domain.Models.Processing;
using MediatR;

namespace ComedyPull.Application.Modules.DataProcessing
{
    public record StateCompletedEvent(Guid BatchId, ProcessingState CompletedState) : INotification;
}