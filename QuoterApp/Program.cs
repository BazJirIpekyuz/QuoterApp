using Microsoft.Extensions.Hosting;
using QuoterApp.DependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddQuoterServices();
        services.AddCaching();
    })
    .Build();

await host.RunAsync();