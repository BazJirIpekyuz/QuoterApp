using Microsoft.Extensions.DependencyInjection;
using QuoterApp.Caching;
using QuoterApp.DependencyInjection;
using QuoterApp.Services;
using System.Threading;

namespace QuoterApp.Tests.Shared
{
    public class QuterAppFixture
    {
        public ServiceProvider ServiceProvider { get; private set; }
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _isCachePopulated;

        public QuterAppFixture()
        {
            ServiceProvider = GetServiceProvider();
        }

        public async Task PopulateCacheWithTestMarketOrdersAsync()
        {
            if (_isCachePopulated)
            {
                return;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_isCachePopulated)
                {
                    return;
                }

                var marketOrderTestDataSource = ServiceProvider.GetRequiredService<IMarketOrderTestDataSource>();
                var marketOrders = marketOrderTestDataSource.GetData();

                var distributedCache = ServiceProvider.GetRequiredService<IDistributedCache<List<MarketOrder>>>();

                foreach (var marketOrder in marketOrders)
                {
                    var instrumentExistingMarketOrders = await distributedCache.GetAsync(marketOrder.InstrumentId) ?? new List<MarketOrder>();
                    instrumentExistingMarketOrders.Add(marketOrder);
                    await distributedCache.SetAsync(marketOrder.InstrumentId, instrumentExistingMarketOrders, -1);
                }

                _isCachePopulated = true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private ServiceProvider GetServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging()
                .AddCaching()
                .AddSingleton<IMarketOrderSource, MarketOrderTestDataSource>()
                .AddSingleton<IMarketOrderTestDataSource, MarketOrderTestDataSource>()
                .AddSingleton<IQuoter, YourQuoter>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
