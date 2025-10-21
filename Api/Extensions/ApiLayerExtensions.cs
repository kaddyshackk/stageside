namespace ComedyPull.Api.Extensions
{
    public static class ApiLayerExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddHttpClient();
        }
    }
}
