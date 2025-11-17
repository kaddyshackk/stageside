namespace StageSide.Scheduler.Service.Scheduling.Operations
{
    public interface IEndpoint
    {
        static abstract void Map(IEndpointRouteBuilder app);
    }
}