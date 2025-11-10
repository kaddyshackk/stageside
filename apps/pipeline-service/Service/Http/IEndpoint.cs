namespace ComedyPull.Service.Http
{
    public interface IEndpoint
    {
        static abstract void Map(IEndpointRouteBuilder app);
    }
}