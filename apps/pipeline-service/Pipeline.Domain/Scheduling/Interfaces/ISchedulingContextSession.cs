using StageSide.Data.ContextSession;
using StageSide.Pipeline.Domain.Scheduling.Models;

namespace StageSide.Pipeline.Domain.Scheduling.Interfaces
{
    public interface ISchedulingContextSession : IContextSession, IDisposable
    {
        IRepository<Schedule> Schedules { get; }
        IRepository<Job> Jobs { get; }
        IRepository<Sitemap> Sitemaps { get; }
    }
}