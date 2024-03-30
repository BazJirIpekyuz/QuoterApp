using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuoterApp.Caching;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuoterApp.Services
{
    public class MarketOrderSourceReadingService : BackgroundService
    {
        private readonly IMarketOrderSource _marketOrderSource;
        private readonly IDistributedCache<List<MarketOrder>> _distributedCache;
        private readonly ILogger<MarketOrderSourceReadingService> _logger;
        // TODO: Take timeout value from a config file.
        private readonly int _getNextMarketOrderTimeout = 1500;

        public MarketOrderSourceReadingService(
            IMarketOrderSource marketOrderSource,
            IDistributedCache<List<MarketOrder>> distributedCache,
            ILogger<MarketOrderSourceReadingService> logger)
        {
            _marketOrderSource = marketOrderSource;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started market order reading service.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var marketOrders = GetMarketOrdersFromSource();
                    await AddMarketOrdersToCache(marketOrders);

                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical exception was thrown.");
            }
        }

        private async Task AddMarketOrdersToCache(IEnumerable<MarketOrder> marketOrders)
        {
            foreach (var marketOrder in marketOrders)
            {
                var instrumentExistingMarketOrders = await _distributedCache.GetAsync(marketOrder.InstrumentId) ?? new List<MarketOrder>();
                instrumentExistingMarketOrders.Add(marketOrder);
                await _distributedCache.SetAsync(marketOrder.InstrumentId, instrumentExistingMarketOrders, -1);
            }
        }

        private IEnumerable<MarketOrder> GetMarketOrdersFromSource()
        {
            using var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            while (true)
            {
                Task<MarketOrder> task = Task<MarketOrder>.Factory.StartNew(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return _marketOrderSource.GetNextMarketOrder();
                }, tokenSource.Token);

                task.Wait(_getNextMarketOrderTimeout);

                if (task.IsCompletedSuccessfully)
                {
                    yield return task.Result;
                }
                else
                {
                    // Cancel task since timeout passed.
                    tokenSource.Cancel();
                    yield break;
                }
            }
        }
    }
}
