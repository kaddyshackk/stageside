using StageSide.Data.ContextSession;
using StageSide.Domain.Models;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface IComedyContextSession : IContextSession, IDisposable
    {
        IRepository<Act> Acts { get; }
        IRepository<Venue> Venues { get; }
        IRepository<Event> Events { get; }
        IRepository<EventAct> EventActs { get; }
    }
}