module Routes

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Requests

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

let test () =
    printfn "Test"
    "illmatic" 

let routes =
    choose [
        host <| test () >=>
            choose [
                route "/ping" >=> show ()
                route "/"     >=> htmlFile "webroot/index.html" ]
        secureHost "fritz.uriegel.de" >=> text "Zur Fritzbox"
        allHosts >=> text "Falscher Host"
    ]

let configureRoutes (app : IApplicationBuilder) = app.UseGiraffe routes    