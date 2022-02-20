module Configuration

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https

open Utils
open Utils.OptionFish

let getPortFromEnvironment = getEnvironmentVariable >=> parseInt 
let httpPort () = getPortFromEnvironment "SERVER_PORT" |> Option.defaultValue 80
let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443

let getLetsEncryptDirectory () = getEnvironmentVariable "LETS_ENCRYPT_DIR"
let makeCertFileName certFile = getLetsEncryptDirectory >=> pathCombine certFile 
let makeCertificatePath = makeCertFileName "cert.pem" 
let makeKeyPath = makeCertFileName "key.pem"
let getCertificate () = makeCertificatePath ()
let getKey () = makeKeyPath () 
let getCertValuePair () = OptionFrom2Options (getCertificate ()) (getKey ())
let getCertificateFromFile = getCertValuePair >=> fun (a, b) -> getCertificateFromFile a b

let httpsOptions (options: HttpsConnectionAdapterOptions) = 
    options.ServerCertificate <- getCertificateFromFile () |> Option.defaultValue null

let httpsListenOptions (options: ListenOptions) = options.UseHttps(httpsOptions)|> ignore
let configureKestrel (options: KestrelServerOptions) = 
    options.ListenAnyIP(httpPort ())
    options.ListenAnyIP(httpsPort (), httpsListenOptions)