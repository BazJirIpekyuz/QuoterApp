using Microsoft.Extensions.DependencyInjection;
using QuoterApp.Tests.Shared;

namespace QuoterApp.Tests
{
    [Collection(TestConstants.QuoterAppCollectionDefinition)]
    public class YourQuoterTests
    {
        private readonly QuterAppFixture _quoterAppFixture;
        private readonly IQuoter _quoter;

        public YourQuoterTests(QuterAppFixture quoterAppFixture)
        {
            _quoterAppFixture = quoterAppFixture;
            _quoter = quoterAppFixture.ServiceProvider.GetRequiredService<IQuoter>();
        }

        [Theory]
        [InlineData("BA79603015", 1, 98)]
        [InlineData("BA79603015", 20, 2056.364)]
        [InlineData("AB73567490", 1, 95.5)]
        [InlineData("AB73567490", 15, 1437.7)]
        public async Task Given_InstrumentAndQuantity_When_MarketHasEnoughQuantity_Then_Return_BestAvailableTotalPriceForGivenQtyAsync(string instrumentId, int quantity, double bestAvailableTotalPrice)
        {
            // Arrange
            await _quoterAppFixture.PopulateCacheWithTestMarketOrdersAsync();

            // Act
            var result = await _quoter.GetQuote(instrumentId, quantity);

            // Assert
            Assert.Equal(bestAvailableTotalPrice, result, 3);
        }

        [Theory]
        [InlineData("BA79603015", 103.095397260274)]
        [InlineData("DK50782120", 99.94012869038607)]
        public async Task Given_Instrument_When_MarketHasInstrument_Then_Return_VolumeWeightedAveragePriceForGivenIntrumentAsync(string instrumentId, double volumeWeightedAveragePrice)
        {
            // Arrange
            await _quoterAppFixture.PopulateCacheWithTestMarketOrdersAsync();

            // Act
            var result = await _quoter.GetVolumeWeightedAveragePrice(instrumentId);

            // Assert
            Assert.Equal(volumeWeightedAveragePrice, result, 3);
        }


        // TODOS: Add tests for expections and validations...

    }
}