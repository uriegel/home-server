open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting
open System
open System.Text.Encodings.Web;
open System.Text.Json
open System.Text.Json.Serialization

open Logging
open Routes

printfn "Launching home server..."

let configureServices (services : IServiceCollection) = 
    let jsonOptions = JsonSerializerOptions()
    jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonOptions.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    jsonOptions.Converters.Add(JsonFSharpConverter())
    jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
    services
        .AddResponseCompression()
        .AddGiraffe()
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

