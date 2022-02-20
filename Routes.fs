module Routes

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Configuration
open Requests

type Msg = {
    Nachricht: string
    Nummer: int
    Versuche: int option
}

let configureRoutes (app : IApplicationBuilder) = 
    let skip : HttpFuncResult = Task.FromResult None

    let host (host: string) (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host with
        | value when value = host -> next ctx
        | _ -> skip

    let insecureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, false when value = host -> next ctx
        | _ -> skip

    let secureHost host (next: HttpFunc) (ctx: HttpContext) =
        match ctx.Request.Host.Host, ctx.Request.IsHttps with
        | value, true when value = host -> next ctx
        | _ -> skip

    let allHosts (next: HttpFunc) (ctx: HttpContext) =
        next ctx
    
    let routes =
        choose [
            host <| (getIntranetHost () |> Option.defaultValue "") >=>
                choose [
                    route "/ping" >=> show ()
                    route "/json" >=> json { Nachricht = "Guten Tag"; Nummer = 9865; Versuche = None }
                    route "/"     >=> htmlFile "webroot/index.html" ]
            secureHost "fritz.uriegel.de" >=> text "Zur Fritzbox"
            allHosts >=> text "Falscher Host"
        ]
    
    app.UseGiraffe routes    