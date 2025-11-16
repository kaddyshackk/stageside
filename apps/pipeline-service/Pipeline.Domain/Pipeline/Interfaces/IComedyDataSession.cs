using StageSide.Domain.Models;
using StageSide.Pipeline.Domain.Interfaces;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface IComedyDataSession : IDataSession, IDisposable
    {
        IRepository<Act> Acts { get; }
        IRepository<Venue> Venues { get; }
        IRepository<Event> Events { get; }
        IRepository<EventAct> EventActs { get; }
    }
}