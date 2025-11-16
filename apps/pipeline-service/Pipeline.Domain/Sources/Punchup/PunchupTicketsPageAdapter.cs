using Microsoft.Extensions.DependencyInjection;
using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.PipelineAdapter;

namespace StageSide.Pipeline.Domain.Sources.Punchup
{
    public class PunchupTicketsPageAdapter(IServiceProvider provider) : IPipelineAdapter
    {
        public IScheduler GetScheduler()
        {
            return provider.GetRequiredService<GenericSitemapScheduler>();
        }

        public ICollector GetCollector()
        {
            return provider.GetRequiredService<PunchupTicketsPageCollector>();
        }

        public ITransformer GetTransformer()
        {
            return new PunchupTicketsPageTransformer();
        }
    }
}