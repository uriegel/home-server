module Utils

open Giraffe
open Microsoft.AspNetCore.Http
open System.Security.Cryptography.X509Certificates

type Response<'a> = 
    | Ok  of 'a
    | Err of System.Exception

let (>=>?) switch1 switch2 x =
    match switch1 x with
    | Some s -> switch2 s
    | None   -> None

let (>=>!) switch1 switch2 x =
    match switch1 x with
    | Ok s -> switch2 s
    | Err e   -> Err e

let switch f x = f x |> Some 

let switchResponse f x = f x |> Ok 

let tee f x =
    f x |> ignore
    x       

let OptionFrom2Options a b = 
    match a, b with
    | Some a, Some b -> Some (a, b)
    | _              -> None

let withInputVar switch x = 
    match switch x with
    | Some s -> Some(x, s)
    | None   -> None

let omitInputVar (_, b)  = Some(b)

let memoize func =
    let memoization = System.Collections.Generic.Dictionary<_, _>()
    fun key ->
        match memoization.TryGetValue key with
        | true, value -> value
        | _           -> let value = func key  
                         memoization.Add(key, value)
                         value

let exceptionToOption func =
    try
        match func () with
        | res when res <> null -> Some(res) 
        | _                    -> None
    with
    | _ -> None

let exceptionToResponse func =
    try
        Ok(func ()) 
    with
    | e -> Err(e)

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

// TODO from FSharpUtils
let icompare a b = 
    System.String.Compare (a, b, System.StringComparison.CurrentCultureIgnoreCase)

// TODO Giraffe
let skip (_: HttpFunc) (__: HttpContext) = System.Threading.Tasks.Task.FromResult None

let httpHandlerParam httpHandler param: HttpHandler = (fun () -> httpHandler(param))()

let routePathes () (routeHandler : string -> HttpHandler) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        Some (SubRouting.getNextPartOfPath ctx)
        |> function
            | None      -> skipPipeline
            | Some subpath -> routeHandler subpath[1..] next ctx    
