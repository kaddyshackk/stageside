using StageSide.Data.Database;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Domain.Database
{
    public interface ISchedulingDbContextSession : IContextSession, IDisposable
    {
        IRepository<Schedule> Schedules { get; }
        IRepository<Job> Jobs { get; }
        IRepository<Source> Sources { get; }
        IRepository<Sku> Skus { get; }
    }
}