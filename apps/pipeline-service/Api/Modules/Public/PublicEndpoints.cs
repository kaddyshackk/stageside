namespace ComedyPull.Api.Modules.Public
{
    public static class PublicEndpoints
    {
        public static IEndpointRouteBuilder AddPublicEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.AddJobEndpoints();
            builder.AddEventEndpoints();

            return builder;
        }
    }
}