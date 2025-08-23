namespace Api.Startup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Infrastructure services
            services.AddCoreServices(configuration);

            // Feature services


            return services;
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }


    }
}
