using Microsoft.Extensions.Logging;
using QuoterApp.Caching;
using QuoterApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoterApp
{
    public class YourQuoter : IQuoter
    {
        private readonly IDistributedCache<List<MarketOrder>> _distributedCache;
        private readonly ILogger<YourQuoter> _logger;

        public YourQuoter(
            IDistributedCache<List<MarketOrder>> distributedCache,
            ILogger<YourQuoter> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<double> GetQuoteAsync(string instrumentId, int quantity)
        {
            ValidateIntrumentId(instrumentId);
            ValidateQuantity(quantity);

            List<MarketOrder> marketOrdersForInstrument = await GetMarketOrdersFromCacheAsync(instrumentId);

            if (marketOrdersForInstrument == null)
            {
                throw new MarketOrderNotFoundException($"Market order not found for instrument id={instrumentId}");
            }

            int instrumentQuantityInMarket = marketOrdersForInstrument.Sum(q => q.Quantity);

            if (quantity > instrumentQuantityInMarket)
            {
                throw new InsufficientQuantityInMarketException($"Insufficient instrument quantity in the market (Wanted: {quantity}, Actual:{instrumentQuantityInMarket}).");
            }

            var marketOrdersOrderedByLowestPrice = marketOrdersForInstrument.OrderBy(q => q.Price).ToList();

            double bestTotalPrice = 0;

            foreach (var marketOrder in marketOrdersOrderedByLowestPrice)
            {
                if (marketOrder.Quantity >= quantity)
                {
                    bestTotalPrice += quantity * marketOrder.Price;
                    break;
                }

                bestTotalPrice += marketOrder.Quantity * marketOrder.Price;
                quantity -= marketOrder.Quantity;
            }

            return bestTotalPrice;
        }

        public async Task<double> GetVolumeWeightedAveragePrice(string instrumentId)
        {
            ValidateIntrumentId(instrumentId);

            var marketOrdersForInstrument = await GetMarketOrdersFromCacheAsync(instrumentId);

            if (marketOrdersForInstrument == null)
            {
                throw new MarketOrderNotFoundException($"Market order not found for instrument id={instrumentId}");
            }

            var vwap = marketOrdersForInstrument.Sum(q => q.Quantity * q.Price) /
                marketOrdersForInstrument.Sum(q => q.Quantity);

            return vwap;
        }

        private static void ValidateIntrumentId(string instrumentId)
        {
            if (string.IsNullOrEmpty(instrumentId)) { throw new ArgumentNullException(nameof(instrumentId)); }
        }

        private void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }
        }

        private async Task<List<MarketOrder>> GetMarketOrdersFromCacheAsync(string instrumentId)
        {
            try
            {
                return await _distributedCache.GetAsync(instrumentId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Cache service is not reachable.");
                throw;
            }
        }
    }
}
