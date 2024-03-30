using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuoterApp.Caching;

namespace QuoterApp.DependencyInjection
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.TryAddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>)); // open generic registration

            return services;
        }
    }
}
