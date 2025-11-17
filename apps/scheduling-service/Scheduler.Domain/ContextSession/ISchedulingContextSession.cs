using StageSide.Data.ContextSession;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Domain.ContextSession
{
    public interface ISchedulingContextSession : IContextSession, IDisposable
    {
        IRepository<Schedule> Schedules { get; }
        IRepository<Job> Jobs { get; }
        IRepository<Sitemap> Sitemaps { get; }
    }
}