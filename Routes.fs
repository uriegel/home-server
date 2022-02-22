module Routes

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Configuration
open Requests
open Utils

let configureRoutes (app : IApplicationBuilder) = 
    let skip : HttpFuncResult = Task.FromResult None

    let host (host: string) (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host with
        | value when value = host -> next ctx
        | _                       -> skip

    let insecureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, false when value = host -> next ctx
        | _                              -> skip

    let secureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, true when value = host -> next ctx
        | _ -> skip

    let allHosts (next: HttpFunc) (ctx: HttpContext) =
        next ctx

    let videoPath = getVideoPath () |> Option.defaultValue ""
    let musicPath = getMusicPath () |> Option.defaultValue ""
    let getVideo =  getVideoFile videoPath


    let routefu () (routeHandler : string -> HttpHandler) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            Some (SubRouting.getNextPartOfPath ctx)
            |> function
                | None      -> skipPipeline
                | Some args -> routeHandler args next ctx    

    let routes =
        choose [
            host <| (getIntranetHost () |> Option.defaultValue "") >=>
                choose [  
                    route  "/media/video/list"     >=> warbler (fun _ -> getVideoList videoPath)
                    routef "/media/video/%s"        <| httpHandlerParam getVideo
                    subRoute "/media/music"
                        (choose [
                            routefu ()              <| httpHandlerParam getMusicList 
                        ])                      
                    route  "/"                     >=> htmlFile "webroot/index.html" 
                ]       
            secureHost "fritz.uriegel.de"          >=> text "Zur Fritzbox"
            allHosts                               >=> text "Falscher Host"
        ]
    
    app.UseGiraffe routes      
    