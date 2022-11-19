module Configuration

open FSharpTools
open FSharpTools.Option
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https

open Utils
open Directory

let getIntranetHost     () = getEnvironmentVariable "INTRANET_HOST"
let getVideoPath        () = getEnvironmentVariable "VIDEO_PATH"
let getPicturePath      () = getEnvironmentVariable "PICTURE_PATH"
let getMusicPath        () = getEnvironmentVariable "MUSIC_PATH"
let getLetsEncryptPath  () = getEnvironmentVariable "LETS_ENCRYPT_DIR"
let getPortFromEnvironment = getEnvironmentVariable >=> String.parseInt 
let getUsbPort          () = getPortFromEnvironment "USB_MEDIA_PORT" 

let configureKestrel (options: KestrelServerOptions) = 
    let getCertificateFromFile = 
        let makeCertFileName certFile = 
            let combineWithCertFile = attachSubPath certFile 
            getLetsEncryptPath >> Option.map combineWithCertFile
        let getCertificate (file: string) = Some(new System.Security.Cryptography.X509Certificates.X509Certificate2(file, "uriegel"))
        makeCertFileName "certificate.pfx" >=> getCertificate

    let getCertificate () = 
        // TODO Memoize this call, reset it every day
        getCertificateFromFile () |> Option.defaultValue null
        
    let httpsOptions (options: HttpsConnectionAdapterOptions) = 
        options.ServerCertificateSelector <- fun a b -> getCertificate ()
    let httpsListenOptions (options: ListenOptions) = 
        options.Protocols <- HttpProtocols.Http1AndHttp2AndHttp3    
        options.UseHttps(httpsOptions) |> ignore
    
    let httpPort  () = getPortFromEnvironment "SERVER_PORT"     |> Option.defaultValue 80
    let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443
    
    options.ListenAnyIP(httpPort ())
    try
        options.ListenAnyIP(httpsPort (), httpsListenOptions)
    with
        | e -> printfn "HTTPS error: %s" <| e.ToString () 



