open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

printfn "Launching home server..."

let webApp =
    choose [
        route "/ping" >=> text "pong"
        route "/"     >=> htmlFile "webroot/index.html" ]
let configureApp (app : IApplicationBuilder) = app.UseGiraffe webApp
let configureServices (services : IServiceCollection) = services.AddGiraffe() |> ignore

let webHostBuilder (webHostBuilder: IWebHostBuilder) = 
    webHostBuilder
        .ConfigureKestrel(Configuration.configureKestrel)
        .Configure(configureApp)
        .ConfigureServices(configureServices)
        |> ignore

Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webHostBuilder)
    .Build()
    .Run()

