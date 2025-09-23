using ComedyPull.Domain.Models.Processing;
using MediatR;

namespace ComedyPull.Application.Features.DataProcessing.Events
{
    public record StateCompletedEvent(Guid BatchId, ProcessingState CompletedState) : INotification;
}