using AspNetExtensions;

using static System.Console;

WriteLine("Launching home server...");

var test = Configuration.GetEnvironmentVariable("VIDEO_PATH");
var test2 = Configuration.GetEnvironmentVariable("VIDEO_PATH");

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(19999))
            .ConfigureServices(services =>
                services
                    .When(true, s => s.AddCors())
                    .AddResponseCompression())
            .ConfigureLogging(builder =>
                builder
                    .AddFilter(a => a == LogLevel.Warning)
                    .AddConsole()
                    .AddDebug()))
    .Build()
    .WithResponseCompression()

    .WithRouting()
    .WithFileServer("", "webroot")
    .Run();
