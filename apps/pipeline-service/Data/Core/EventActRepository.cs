using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Core.Events;
using ComedyPull.Domain.Core.Events.Interfaces;

namespace ComedyPull.Data.Core
{
    /// inheritdoc
    public class EventActRepository(ComedyDbContext context) : IEventActRepository
    {
        /// inheritdoc
        public async Task BulkCreateEventActsAsync(IEnumerable<EventAct> eventActs, CancellationToken stoppingToken)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.EventActs.AddRange(eventActs);
                await context.SaveChangesAsync(stoppingToken);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }
    }
}