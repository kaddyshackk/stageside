using ComedyPull.Domain.Jobs;

namespace ComedyPull.Domain.Interfaces.Data
{
    public interface IPipelineDataSession : IDataSession, IDisposable
    {
        IRepository<Job> Jobs { get; }
        IRepository<Execution> Executions { get; }
        IRepository<Sitemap> Sitemaps { get; }
    }
}