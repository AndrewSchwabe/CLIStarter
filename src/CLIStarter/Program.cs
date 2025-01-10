using System;
using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (context, services) =>
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            services
                .AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true,
                    reloadOnChange: true
                )
                .Build();

            services.AddSingleton(configuration);
            services.AddHttpClient();

            // Register dependencies here
        }
    )
    .Build();

// Register Arguments and options here
var argument = new Argument<string>("argument", description: "The first argument");

var rootCommand = new RootCommand { argument };

// Configure handlers here
rootCommand.SetHandler(
    (argument) =>
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(argument);
    },
    argument
);

await rootCommand.InvokeAsync(args);
