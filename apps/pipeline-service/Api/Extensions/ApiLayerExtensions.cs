namespace ComedyPull.Api.Extensions
{
    public static class ApiLayerExtensions
    {
        public static void AddApiLayer(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddHttpClient();
        }
    }
}
