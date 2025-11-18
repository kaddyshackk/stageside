using StageSide.Data.Database;
using StageSide.Domain.Models;

namespace StageSide.Processor.Domain.Database
{
    public interface IComedyDbContextSession : IContextSession, IDisposable
    {
        IRepository<Act> Acts { get; }
        IRepository<Venue> Venues { get; }
        IRepository<Event> Events { get; }
        IRepository<EventAct> EventActs { get; }
    }
}