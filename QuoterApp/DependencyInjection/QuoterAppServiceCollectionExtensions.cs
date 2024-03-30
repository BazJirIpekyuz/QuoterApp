using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using QuoterApp.Services;

namespace QuoterApp.DependencyInjection
{
    public static class QuoterAppServiceCollectionExtensions
    {
        public static IServiceCollection AddQuoterServices(this IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddConsole());
            services.AddHostedService<MarketOrderSourceReadingService>();
            services.TryAddSingleton<IMarketOrderSource, HardcodedMarketOrderSource>();
            services.TryAddSingleton<IQuoter, YourQuoter>();

            return services;
        }
    }

}
