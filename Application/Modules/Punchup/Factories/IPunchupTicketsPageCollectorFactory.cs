using ComedyPull.Application.Modules.Punchup.Collectors;

namespace ComedyPull.Application.Modules.Punchup.Factories
{
    public interface IPunchupTicketsPageCollectorFactory
    {
        PunchupTicketsPageCollector CreateCollector();
    }
}