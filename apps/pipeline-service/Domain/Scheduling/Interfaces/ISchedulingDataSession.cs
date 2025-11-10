using ComedyPull.Domain.Interfaces;
using ComedyPull.Domain.Scheduling.Models;

namespace ComedyPull.Domain.Scheduling.Interfaces
{
    public interface ISchedulingDataSession : IDataSession, IDisposable
    {
        IRepository<Job> Jobs { get; }
        IRepository<Execution> Executions { get; }
        IRepository<Sitemap> Sitemaps { get; }
    }
}