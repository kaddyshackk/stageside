using ComedyPull.Api.Options;

namespace ComedyPull.Api.Extensions
{
    public static class ApiLayerExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));
            services.AddEndpointsApiExplorer();
            services.AddHttpClient();
        }
    }
}
