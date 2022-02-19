module Configuration

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https
open Utils.OptionFish

let getPortFromEnvironment = Utils.getEnvironmentVariable >=> Utils.parseInt 

let httpPort () = getPortFromEnvironment "SERVER_PORT" |> Option.defaultValue 80
let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443

let httpsOptions (options: HttpsConnectionAdapterOptions) = ()
let httpsListenOptions (options: ListenOptions) = options.UseHttps(httpsOptions)|> ignore
let configureKestrel (options: KestrelServerOptions) = 
    options.ListenAnyIP(httpPort ())
    options.ListenAnyIP(httpsPort (), httpsListenOptions)