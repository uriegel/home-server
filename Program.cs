using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
                    .GetOrDefault(8080)
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
                    .GetEnvironmentVariable("SERVER_TLS_PORT")
                    .SelectMany(StringExtensions.ParseInt)
                    .GetOrDefault(4433), options => {
                        options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                        options.UseHttps(options => options.ServerCertificateSelector = (a, b) => null);
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
        .WithMapGet("/web", () => "Das ist die Standard Webseite")
        .GetApp()
    .Start();
