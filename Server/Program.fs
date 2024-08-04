open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System
open System.Text.Encodings.Web
open System.Text.Json
open System.Text.Json.Serialization

open Logging
open Routes

DatabaseAccess.test ()


printfn "Launching home server..."

let configureServices (services : IServiceCollection) = 
    let jsonOptions = JsonSerializerOptions()
    jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonOptions.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    jsonOptions.Converters.Add(JsonFSharpConverter())
    jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
    services
        .AddSingleton(jsonOptions) 
        

// TODO check Giraffe 7.0.0
//        .AddSingleton<Json.ISerializer, SystemTextJson.Serializer>() 


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

