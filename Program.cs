using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;

using static System.Console;
using static Configuration;
using static Requests;
using static DiskAccess;

WriteLine("Launching home server...");

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(
                GetEnvironmentVariable(ServerPort)
                .SelectMany(StringExtensions.ParseInt)
                .GetOrDefault(80)
            ))
            .ConfigureKestrel(options => options.Limits.MaxRequestBodySize = null)
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
    .WithHost(GetEnvironmentVariable(IntranetHost).GetOrDefault(Environment.MachineName))
        .WithMapGet("/media/video/{**path}", ServeVideo)
        .WithMapGet("/media/pics/{**path}", ServePictures)
        .WithMapGet("/media/music/{**path}", ServeMusic)
        .WithMapGet("/media/accessdisk", AccessDisk)
        .WithMapGet("/media/diskneeded", DiskNeeded)
        .WithJsonPost<CommanderEngine.Input, CommanderEngine.RemoteItem[]>("/remote/getfiles", CommanderEngine.GetFiles)
        .WithMapPost("remote/getfile", CommanderEngine.GetFile)
        .WithMapPost("remote/postfile", CommanderEngine.PostFile)
        .WithMapGet("/remote/{**path}", CommanderEngine.Serve)
        .GetApp()
    .WithHost("fritz.uriegel.de")
        .LetsEncrypt()
        .GetApp()
    .WithHost("familie.uriegel.de")
        .WithMapGet("/hochzeit", c => GetZipFile(c, "hochzeit.zip"))
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
        .WithReverseProxy("", "http://fritz.box")
        .GetApp()
    .WithHost("familie.uriegel.de")
        .GetApp()
    .WithHost("uriegel.de")
        .WithMapGet("/web", () => "Default Web Site")
        .GetApp()
    .Run();
