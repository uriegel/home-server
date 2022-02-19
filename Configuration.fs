module Configuration

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https
open Utils.OptionFish

let OptionFrom2Options a b = 
    match (a, b) with
    | (Some a, Some b) -> Some (a, b)
    | _ -> None

let getPortFromEnvironment = Utils.getEnvironmentVariable >=> Utils.parseInt 
let httpPort () = getPortFromEnvironment "SERVER_PORT" |> Option.defaultValue 80
let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443

let getLetsEncryptDirectory () = Utils.getEnvironmentVariable "LETS_ENCRYPT_DIR"
let appendCertFile certFile = Utils.pathCombine certFile

let makeCertFileName certFile = getLetsEncryptDirectory >=> Utils.pathCombine certFile // |> Option.defaultValue ""
let makeCertificatePath = makeCertFileName "cert.pem" //|> Option.defaultValue ""
let makeKeyPath = makeCertFileName "key.pem"
let getCertificate () = makeCertificatePath ()
let getKey () = makeKeyPath () 
let getCertValuePair () = OptionFrom2Options (getCertificate ()) (getKey ())
let getCertificateFromFile = getCertValuePair >=> fun (a, b) -> Utils.getCertificateFromFile a b

let httpsOptions (options: HttpsConnectionAdapterOptions) = 
    options.ServerCertificate <- getCertificateFromFile () |> Option.defaultValue null

let httpsListenOptions (options: ListenOptions) = options.UseHttps(httpsOptions)|> ignore
let configureKestrel (options: KestrelServerOptions) = 
    options.ListenAnyIP(httpPort ())
    options.ListenAnyIP(httpsPort (), httpsListenOptions)