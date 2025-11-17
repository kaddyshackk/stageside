using StageSide.Pipeline.Interfaces;

namespace StageSide.Punchup.Adapter
{
    public class PunchupTicketsPageAdapter(IServiceProvider provider) : IPipelineAdapter
    {
        public IScheduler GetScheduler()
        {
            throw new NotImplementedException();
        }

        public ICollector GetCollector()
        {
            throw new NotImplementedException();
        }

        public ITransformer GetTransformer()
        {
            throw new NotImplementedException();
        }
    }
}