using ComedyPull.Domain.Enums;
using MediatR;

namespace ComedyPull.Application.Events
{
    public record StateCompletedEvent(Guid BatchId) : INotification;
}