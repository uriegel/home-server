using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;

using static System.Console;
using static Configuration;

WriteLine("Launching home server...");

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(
                Configuration
                    .GetEnvironmentVariable(ServerPort)
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
    .WithHost("fritz.uriegel.de")
        .LetsEncrypt()
        .GetApp()
    .WithHost("familie.uriegel.de")
        .LetsEncrypt()
        .GetApp()
    .WithHost("uriegel.de")
        .LetsEncrypt()
        .GetApp()
    .WithFileServer("/test", "webroot")
    .Start();

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(
                Configuration
                    .GetEnvironmentVariable(ServerTlsPort)
                    .SelectMany(StringExtensions.ParseInt)
                    .GetOrDefault(443), options => {
                        options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                        options.UseHttps(options 
                            => options.ServerCertificateSelector = (_, __) 
                                => Certificate.Get());
                    }
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
    .WithHost("fritz.uriegel.de")
        .GetApp()
    .WithHost("familie.uriegel.de")
        .GetApp()
    .WithHost("uriegel.de")
        .WithMapGet("/web", () => "Default Web Site")
        .GetApp()
    .Run();
