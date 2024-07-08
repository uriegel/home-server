using AspNetExtensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using CsTools.Extensions;

using static System.Console;
using static CsTools.WithLogging;
using static Configuration;
using static Requests;
using static DiskAccess;

WriteLine("Launching home server...");

// TODO reject connection when letsencryt is active or useReject

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => 
                options
                    .UseListenAnyIP(
                        GetEnvironmentVariable(ServerPort)
                            ?.ParseInt()
                            ?? 80)
                    .UseLimits(limits => limits.SetMaxRequestBodySize(null)))
            .ConfigureServices(services =>
                services
                    .AddResponseCompression())
            .ConfigureLogging(builder =>
                builder
                    .AddFilter(a => a == LogLevel.Trace)
                    .AddConsole()
                    .AddDebug()))
    .Build()
    .WithResponseCompression()
    .WithRouting()
    .WithHost(GetEnvironmentVariable(IntranetHost) ?? Environment.MachineName)
        .WithMapGet("/media/video/{**path}", ServeVideo)
        .WithMapGet("/media/pics/{**path}", ServePictures)
        .WithMapGet("/media/thumbnail/{**path}", ServeThumbnail)
        .WithMapGet("/media/music/{**path}", ServeMusic)
        .WithMapGet("/media/accessdisk", AccessDisk)
        .WithMapGet("/media/diskneeded", DiskNeeded)
        .WithJsonGet("/getfiles/{**path}", CommanderEngine.GetFiles)
        .WithMapPut("/putfile/{**path}", CommanderEngine.PutFile)
        .WithMapGet("/getfile/{**path}", CommanderEngine.Serve)
        .WithMapGet("/downloadfile/{**path}", CommanderEngine.Serve)
        .GetApp()
    .WithHost("fritz.uriegel.de")
        .UseLetsEncryptValidation()
        .GetApp()
    .WithHost("familie.uriegel.de")
        .UseLetsEncryptValidation()
        .GetApp()
    .WithHost("uriegel.de")
        .UseLetsEncryptValidation()
        .GetApp()
#if DEBUG        
    .WithFileServer("/test", "webroot") // TODO only for hosts != withHost
#endif
    .Start();

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(
                GetEnvironmentVariable(ServerTlsPort)
                ?.ParseInt() ?? 443, options => {
                    options.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                    options.UseHttps(LetsEncrypt.Use);
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
    .WithHost("uriegel.de")
        .WithMapPost("tracker/ping", Tracker.Ping)
        .WithMapGet("/", () => "Under construction")
        .GetApp()
    .WithHost("fritz.uriegel.de")
        .WithReverseProxy("", "http://fritz.box")
        .GetApp()
    .WithHost("familie.uriegel.de")
        .WithMapGet("/hochzeit", c => GetZipFile(c, "hochzeit.zip"))
        .GetApp()
    .Run();
