module Configuration

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.Kestrel.Core
open Microsoft.AspNetCore.Server.Kestrel.Https

open Utils

let getIntranetHost () = getEnvironmentVariable "INTRANET_HOST"
let getVideoPath    () = getEnvironmentVariable "VIDEO_PATH"

let configureKestrel (options: KestrelServerOptions) = 
    let getCertificateFromFile = 
        let makeCertFileName certFile = 
            let getLetsEncryptDirectory () = getEnvironmentVariable "LETS_ENCRYPT_DIR"
            getLetsEncryptDirectory >=>? pathCombine certFile 
        let makeCertificatePath = makeCertFileName "cert.pem" 
        let makeKeyPath = makeCertFileName "key.pem"
        let getCertificate () = makeCertificatePath ()
        let getKey () = makeKeyPath () 
        let getCertValuePair () = OptionFrom2Options (getCertificate ()) (getKey ())
        getCertValuePair >=>? fun (a, b) -> getCertificateFromFile a b
    let httpsOptions (options: HttpsConnectionAdapterOptions) = 
        options.ServerCertificate <- getCertificateFromFile () |> Option.defaultValue null
    let httpsListenOptions (options: ListenOptions) = options.UseHttps(httpsOptions)|> ignore
    let getPortFromEnvironment = getEnvironmentVariable >=>? parseInt 
    let httpPort  () = getPortFromEnvironment "SERVER_PORT"     |> Option.defaultValue 80
    let httpsPort () = getPortFromEnvironment "SERVER_TLS_PORT" |> Option.defaultValue 443

    options.ListenAnyIP(httpPort ())
    options.ListenAnyIP(httpsPort (), httpsListenOptions)   