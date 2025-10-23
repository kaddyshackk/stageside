using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.Punchup.Collectors
{
    public class PunchupTicketsPageCollectorFactory(IQueue<BronzeRecord> queue) : IPunchupTicketsPageCollectorFactory
    {
        public PunchupTicketsPageCollector CreateCollector(Guid batchId)
        {
            return new PunchupTicketsPageCollector(queue, batchId);
        }
    }
}