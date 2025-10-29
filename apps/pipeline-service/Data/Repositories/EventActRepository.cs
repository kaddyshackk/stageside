using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;

namespace ComedyPull.Data.Repositories
{
    /// inheritdoc
    public class EventActRepository(ComedyDbContext context) : IEventActRepository
    {
        /// inheritdoc
        public async Task BulkCreateEventActsAsync(IEnumerable<EventAct> eventActs)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.EventActs.AddRange(eventActs);
                await context.SaveChangesAsync();
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }
    }
}