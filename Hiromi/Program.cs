using Hiromi.Commands;
using Hiromi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Hiromi;

public class Program
{
    public static async Task Main()
    {
        var host = CreateHost().Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .WriteTo.Console(theme: SystemConsoleTheme.Literate);
            })
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true);
            })
            .ConfigureServices((context, collection) =>
            {
                collection.Configure<DiscordGatewayClientOptions>(g => g.Intents |= GatewayIntents.MessageContents);
                
                collection
                    .AddDiscordCommands()
                    .AddCommandTree()
                    .WithCommandGroup<NekogirlCommands>()
                    .Finish();

                collection.AddSingleton<ITrashgirlService, TrashgirlService>();
            })
            .AddDiscordService(services =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                return configuration["Discord:Token"];
            });
    }
}