using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuoterApp.Caching;
using QuoterApp.Services;
using QuoterApp.Tests.Shared;

namespace QuoterApp.Tests.Services
{
    [Collection(TestConstants.QuoterAppCollectionDefinition)]
    public class MarketOrderSourceReadingServiceTests
    {
        private readonly QuterAppFixture _quoterAppFixture;

        public MarketOrderSourceReadingServiceTests(QuterAppFixture quoterAppFixture)
        {
            _quoterAppFixture = quoterAppFixture;
        }

        [Fact]
        public async Task Given_MarketOrderSource_When_ServiceExecuted_Then_CachePopulatedWithMarketOrderDataAsync()
        {
            // Arrange
            var marketOrderTestDataSource = _quoterAppFixture.ServiceProvider.GetRequiredService<IMarketOrderSource>();
            var distributedCache = _quoterAppFixture.ServiceProvider.GetRequiredService<IDistributedCache<List<MarketOrder>>>();

            var mors = new MarketOrderSourceReadingService(
                marketOrderTestDataSource,
                distributedCache,
                NullLogger<MarketOrderSourceReadingService>.Instance);

            // Act
            await mors.StartAsync(default);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await mors.StopAsync(cts.Token);
            await mors.ExecuteTask;

            // Assert
            Assert.True(mors.ExecuteTask.IsCompletedSuccessfully);

            // Assert if cache has been populated by service.
            var cacheData = await distributedCache.GetAsync("BA79603015");
            Assert.NotNull(cacheData);
            Assert.NotEmpty(cacheData);
        }
    }
}
