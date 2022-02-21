open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Text.Json
    
open Logging
open Routes

printfn "Launching home server..."

let configureServices (services : IServiceCollection) = 
    let jsonOptions = JsonSerializerOptions()
    // TODO FSharpUtils
    //jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
//    jsonOptions.Converters.Add(JsonFSharpConverter())
    services
        .AddGiraffe()
        //.AddSingleton(jsonOptions) 
        //.AddSingleton<Json.ISerializer, SystemTextJson.Serializer>() 
    |> ignore

let webHostBuilder (webHostBuilder: IWebHostBuilder) = 
    webHostBuilder
        .ConfigureKestrel(Configuration.configureKestrel)
        .Configure(configureRoutes)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        |> ignore

Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webHostBuilder)
    .Build()
    .Run()

