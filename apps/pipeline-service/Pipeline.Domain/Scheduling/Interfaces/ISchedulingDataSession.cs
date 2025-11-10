using StageSide.Pipeline.Domain.Interfaces;
using StageSide.Pipeline.Domain.Scheduling.Models;

namespace StageSide.Pipeline.Domain.Scheduling.Interfaces
{
    public interface ISchedulingDataSession : IDataSession, IDisposable
    {
        IRepository<Schedule> Schedules { get; }
        IRepository<Job> Jobs { get; }
        IRepository<Sitemap> Sitemaps { get; }
    }
}