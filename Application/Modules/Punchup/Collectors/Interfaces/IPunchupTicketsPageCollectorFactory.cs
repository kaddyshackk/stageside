namespace ComedyPull.Application.Modules.Punchup.Collectors.Interfaces
{
    public interface IPunchupTicketsPageCollectorFactory
    {
        PunchupTicketsPageCollector CreateCollector();
    }
}