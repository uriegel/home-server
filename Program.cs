using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;

using static System.Console;

WriteLine("Launching home server...");

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(
                Configuration
                    .GetEnvironmentVariable("SERVER_PORT")
                    .SelectMany(StringExtensions.ParseInt)
                    .GetOrDefault(80)
            ))
            .ConfigureServices(services =>
                services
                    .AddResponseCompression())
            .ConfigureLogging(builder =>
                builder
                    .AddFilter(a => a == LogLevel.Warning)
                    .AddConsole()
                    .AddDebug()))
    .Build()
    .WithResponseCompression()
    .WithRouting()
    .WithHost("localhost")
        .GetApp()
    .WithFileServer("/test", "webroot")
    .Run();
