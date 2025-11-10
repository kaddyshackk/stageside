namespace StageSide.Pipeline.Service.Operations
{
    public interface IEndpoint
    {
        static abstract void Map(IEndpointRouteBuilder app);
    }
}