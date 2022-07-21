module Routes

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Configuration
open Requests
open Utils
open GiraffeTools

let configureRoutes (app : IApplicationBuilder) = 
    let host (host: string) (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host with
        | value when value = host -> next ctx
        | _                       -> skipPipeline

    let insecureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, false when value = host -> next ctx
        | _                              -> skipPipeline

    let secureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, true when value = host -> next ctx
        | _                             -> skipPipeline

    let allHosts (next: HttpFunc) (ctx: HttpContext) =
        next ctx

    let videoPath =   getVideoPath ()   |> Option.defaultValue ""
    let picturePath = getPicturePath () |> Option.defaultValue ""
    let musicPath =   getMusicPath ()   |> Option.defaultValue ""
    let getVideo =    getVideoFile videoPath

    let letsEncrypt = 
        choose [  
            routef "/.well-known/acme-challenge/%s"          <| httpHandlerParam getLetsEncryptToken
        ]

    let routes =
        choose [
            host <| (getIntranetHost () |> Option.defaultValue "") >=>
                choose [  
                    route  "/media/video/list" >=> warbler (fun _ -> getVideoList videoPath)
                    routef "/media/video/%s"    <| httpHandlerParam getVideo
                    subRoute "/media/pics"
                        (choose [
                            routePathes ()      <| httpHandlerParam (getFileList picturePath)
                            routePathes ()      <| httpHandlerParam (getPictureFile picturePath)
                        ])                      
                    subRoute "/media/music"
                        (choose [
                            routePathes ()      <| httpHandlerParam (getFileList musicPath)
                            routePathes ()      <| httpHandlerParam (getMusicFile musicPath)
                        ])                      
                    route  "/"           >=> htmlFile "webroot/index.html" 
                ]  
            host "uriegel.de"                  >=> letsEncrypt
            host "fritz.uriegel.de"            >=> letsEncrypt
            host "familie.uriegel.de"          >=> letsEncrypt
            secureHost "fritz.uriegel.de"      >=> ReverseProxy.handler 
            allHosts                           >=> text "Falscher Host"
        ]
    
    app
        .UseResponseCompression()
        .UseGiraffe routes      
    