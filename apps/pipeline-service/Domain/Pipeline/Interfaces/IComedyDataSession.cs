using ComedyPull.Domain.Interfaces;
using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface IComedyDataSession : IDataSession, IDisposable
    {
        IRepository<Act> Acts { get; }
        IRepository<Venue> Venues { get; }
        IRepository<Event> Events { get; }
        IRepository<EventAct> EventActs { get; }
    }
}