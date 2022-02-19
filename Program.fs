open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

printfn "Launching home server..."

let webApp =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "webroot/index.html" ]

let configureApp (app : IApplicationBuilder) = app.UseGiraffe webApp
let configureServices (services : IServiceCollection) = services.AddGiraffe() |> ignore
let configureKestrel (options: KestrelServerOptions) = 
    options.ListenAnyIP(8080)
    options.ListenAnyIP(9865)
let webHostBuilder (webHostBuilder: IWebHostBuilder) = 
    webHostBuilder
        .ConfigureKestrel(configureKestrel)
        .Configure(configureApp)
        .ConfigureServices(configureServices)
        |> ignore

Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webHostBuilder)
    .Build()
    .Run()
    
    