module Requests

open FSharpTools
open Giraffe
open Microsoft.AspNetCore.Http
open System.IO

open Directory

type DirectoryItems = {
    Directories: string[]    
    Files:       string[]    
}

type NotADirectoryException() = inherit System.Exception()

let setContentType contentType (next: HttpFunc) (ctx: HttpContext) =
    ctx.SetHttpHeader("Content-Type", contentType)
    next ctx

open FSharpTools.Result

open Giraffe

let getLetsEncryptToken token = 
    let makeTokenFileName tokenFile = 
        let combineWithToken = attachSubPath tokenFile
        Configuration.getLetsEncryptPath >> Option.map combineWithToken
    let makeTokenPath = makeTokenFileName token 

    let path = makeTokenPath () |> Option.defaultValue ""
    setContentType "text/plain" >=> streamFile false path None None

open FSharpTools.Result
open GiraffeTools

let getDirectoryItems root path =

    let getFileSystemInfos path = 
        let getName n = 
            let getName (fileInfo: FileSystemInfo) = fileInfo.Name
            n :> FileSystemInfo
            |> getName
        let getFiles path = 
            DirectoryInfo(path).GetFiles() 
            |> Array.map getName
            |> Array.sortWith String.icompare
        let getDirectories path = 
            DirectoryInfo(path).GetDirectories() 
            |> Array.map getName
            |> Array.sortWith String.icompare
        let getFileSystemInfos path = { 
            Directories = getDirectories path
            Files = getFiles path
        }
        exceptionToResult (fun () -> getFileSystemInfos path)
    
    let checkDirectory path = if existsDirectory path then Ok(path) else Error(NotADirectoryException() :> System.Exception)
    let getListFromPathParts = 
        combinePathes 
        >> checkDirectory
        >=> getFileSystemInfos 

    match getListFromPathParts [|root; path|] with
    | Ok value                                        -> json { Files = value.Files; Directories = value.Directories }
    | Error e when e :? NotADirectoryException = true -> skip
    // TODO send error html and log error
    | Error _                                         -> text "No output"

open System.Diagnostics
open FSharpTools.Option
open Configuration

let accessDisk () =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            printfn "access disk"
            do! DiskAccess.access () 
            return! text "disk accessed" next ctx
        }

let diskNeeded () =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            printfn "disk needed"
            DiskAccess.needed ()
            return! text "Disk shutdown delayed" next ctx
        }

open Giraffe

let getVideoFile root path =
    let path = [| root; path |] |> Directory.combinePathes  
    setContentType "video/mp4" >=> streamFile true path None None

let getPictureFile root path =
    let path = [| root; path |] |> Directory.combinePathes  
    setContentType "image/jpg" >=> streamFile false path None None

let getMusicFile root path =
    let path = [| root; path |] |> Directory.combinePathes  
    setContentType "audio/mp3" >=> streamFile true path None None

let getPicturesZipFile path =
    let path = [| path; "taufe.zip" |] |> Directory.combinePathes  
    setContentType "application/zip" >=> streamFile false path None None
