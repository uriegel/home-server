module Configuration

open FSharpRailway.Option
open FSharpTools
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https

open Utils
open Directory
open FSharpTools

let getIntranetHost    () = getEnvironmentVariable "INTRANET_HOST"
let getVideoPath       () = getEnvironmentVariable "VIDEO_PATH"
let getPicturePath     () = getEnvironmentVariable "PICTURE_PATH"
let getMusicPath       () = getEnvironmentVariable "MUSIC_PATH"
let getLetsEncryptPath () = getEnvironmentVariable "LETS_ENCRYPT_DIR"

let configureKestrel (options: KestrelServerOptions) = 
    let getCertificateFromFile = 
        let makeCertFileName certFile = 
            let combineWithCertFile = attachSubPath certFile 
            getLetsEncryptPath >> Option.map combineWithCertFile
        let makeCertificatePath = makeCertFileName "cert.pem" 
        let makeKeyPath = makeCertFileName "key.pem"
        let getCertificate () = makeCertificatePath ()
        let getKey () = makeKeyPath () 
        let getCertValuePair () = OptionFrom2Options (getCertificate ()) (getKey ())
        getCertValuePair >=> fun (a, b) -> Security.getCertificateFromFile a b
    let httpsOptions (options: HttpsConnectionAdapterOptions) = 
        options.ServerCertificate <- getCertificateFromFile () |> Option.defaultValue null
    let httpsListenOptions (options: ListenOptions) = 
        options.Protocols <- HttpProtocols.Http1AndHttp2AndHttp3    
        options.UseHttps(httpsOptions) |> ignore
    let getPortFromEnvironment = getEnvironmentVariable >=> String.parseInt 
    let httpPort  () = getPortFromEnvironment "SERVER_PORT"     |> Option.defaultValue 80
    let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443

    options.ListenAnyIP(httpPort ())
    try
        options.ListenAnyIP(httpsPort (), httpsListenOptions)
    with
        | e -> printfn "HTTPS error: %s" <| e.ToString () 
