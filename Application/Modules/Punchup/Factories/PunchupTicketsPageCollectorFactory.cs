using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Modules.Punchup.Factories
{
    public class PunchupTicketsPageCollectorFactory : IPunchupTicketsPageCollectorFactory
    {
        private readonly IQueue<SourceRecord> _queue;

        public PunchupTicketsPageCollectorFactory(IQueue<SourceRecord> queue)
        {
            _queue = queue;
        }

        public PunchupTicketsPageCollector CreateCollector()
        {
            return new PunchupTicketsPageCollector(_queue);
        }
    }
}