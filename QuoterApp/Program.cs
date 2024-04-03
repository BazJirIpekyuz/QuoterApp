using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuoterApp.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuoterApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                using var scope = host.Services.CreateScope();

                var serviceProvider = scope.ServiceProvider;

                var marketOrderReadingService = serviceProvider.GetRequiredService<IHostedService>();
                await marketOrderReadingService.StartAsync(default);

                Console.WriteLine($"Started reading market orders and populating cache, please wait...");
                // Give time for market order reading service to read source and populate cache.
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Console.WriteLine($"Finished reading market orders.");

                Console.WriteLine($"******************");

                var gq = serviceProvider.GetRequiredService<IQuoter>();
                var qty = 120;

                var quote = await gq.GetQuote("DK50782120", qty);
                var vwap = await gq.GetVolumeWeightedAveragePrice("DK50782120");

                Console.WriteLine($"Quote: {quote}, {quote / (double)qty}");
                Console.WriteLine($"Average Price: {vwap}");
                Console.WriteLine();
                Console.WriteLine($"Done");

                Console.WriteLine($"******************");

                await host.RunAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddQuoterServices();
                    services.AddCaching();
                });
        }
    }
}
