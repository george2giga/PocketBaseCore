using Microsoft.Extensions.DependencyInjection;

namespace PocketBaseCore
{
    public static class ServiceCollectionExtensions
    {
        //register services
        public static void RegisterPocketSharp(this IServiceCollection services)
        {
            services.AddScoped<IPocketSharpClient, PocketSharpClient>();
        }
    }
}