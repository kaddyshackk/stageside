using ComedyPull.Domain.Core.Acts;
using ComedyPull.Domain.Core.Events;
using ComedyPull.Domain.Core.Venues;

namespace ComedyPull.Domain.Interfaces.Data
{
    public interface IComedyDataSession : IDataSession, IDisposable
    {
        IRepository<Act> Acts { get; }
        IRepository<Venue> Venues { get; }
        IRepository<Event> Events { get; }
        IRepository<EventAct> EventActs { get; }
    }
}