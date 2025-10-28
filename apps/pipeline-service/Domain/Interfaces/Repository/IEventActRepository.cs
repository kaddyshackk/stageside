using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Repository
{
    public interface IEventActRepository
    {
        public Task BulkCreateEventActsAsync(IEnumerable<EventAct> eventActs);
    }
}