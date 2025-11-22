namespace StageSide.SpaCollector.Service.Operations
{
    public interface IEndpoint
    {
        static abstract void Map(IEndpointRouteBuilder app);
    }
}