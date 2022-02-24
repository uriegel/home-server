module Utils

open Giraffe
open FSharpTools.Functional
open Microsoft.AspNetCore.Http
open System.Security.Cryptography.X509Certificates

let parseInt (str: string) = 
    match System.Int32.TryParse str with
    | true, int -> Some int
    | _         -> None

let retrieveEnvironmentVariable key =
    exceptionToOption (fun () -> System.Environment.GetEnvironmentVariable key)  

let getEnvironmentVariableLogged =
    let logToConsole (key, value) = printfn "Reading environment %s: %s" key value
    withInputVar retrieveEnvironmentVariable 
        >=>? switch (tee logToConsole) 
        >=>? omitInputVar

let getEnvironmentVariable = memoize getEnvironmentVariableLogged

let getCertificateFromFile certPath keyPath =
    exceptionToOption (fun () -> X509Certificate2.CreateFromPemFile (certPath, keyPath))

let pathCombine subPath path =
    exceptionToOption (fun () -> System.IO.Path.Combine (path, subPath))

let getFiles path = 
    exceptionToResponse (fun () -> System.IO.DirectoryInfo(path).GetFiles())

let getDirectories path = 
    exceptionToResponse (fun () -> System.IO.DirectoryInfo(path).GetDirectories())

let getFileSystemInfos path = 
    let getAsInfo n = n :> System.IO.FileSystemInfo
    let getFiles path = System.IO.DirectoryInfo(path).GetFiles() |> Array.map getAsInfo
    let getDirectories path = System.IO.DirectoryInfo(path).GetDirectories() |> Array.map getAsInfo
    let getFileSystemInfos path = Array.concat [|getFiles path; getDirectories path |] 
    exceptionToResponse (fun () -> getFileSystemInfos path)

let existsFile file = System.IO.File.Exists file    
let getExistingFile file = if existsFile file then Some file else None 
let combinePath (pathes: string[]) = exceptionToResponse (fun () -> System.IO.Path.Combine pathes)
let isDirectory (path: string) = System.IO.Directory.Exists path

// TODO Giraffe
let skip (_: HttpFunc) (__: HttpContext) = System.Threading.Tasks.Task.FromResult None

let httpHandlerParam httpHandler param: HttpHandler = (fun () -> httpHandler(param))()

let routePathes () (routeHandler : string -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        Some (SubRouting.getNextPartOfPath ctx)
        |> function
            | Some subpath -> routeHandler subpath[1..] next ctx    
            | None         -> skipPipeline
