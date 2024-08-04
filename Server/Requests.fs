module Requests

open System.IO
open Microsoft.AspNetCore.Http
open FSharpPlus
open FSharpTools
open FSharpTools.Directory

type DirectoryItems = {
    Directories: string[]    
    Files:       string[]    
}

open Giraffe

type NotADirectoryException() = inherit System.Exception()

let setContentType contentType (next: HttpFunc) (ctx: HttpContext) =
    ctx.SetHttpHeader("Content-Type", contentType)
    next ctx

let getLetsEncryptToken token = 
    let makeTokenFileName tokenFile = 
        let combineWithToken = attachSubPath tokenFile
        Configuration.getLetsEncryptPath >> Option.map combineWithToken
    let makeTokenPath = makeTokenFileName token 

    let path = makeTokenPath () |> Option.defaultValue ""
    setContentType "text/plain" >=> streamFile false path None None

open FSharpPlus
open FSharpTools

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
        Result.catch (fun () -> getFileSystemInfos path)
    
    let checkDirectory path = if existsDirectory path then Ok(path) else Error(NotADirectoryException() :> System.Exception)
    let getListFromPathParts = 
        combinePathes 
        >> checkDirectory
        >=> getFileSystemInfos 

    match getListFromPathParts [|root; path|] with
    | Ok value -> json { Files = value.Files; Directories = value.Directories }
    | Error e when e :? NotADirectoryException = true -> GiraffeTools.skip
    // TODO send error html and log error
    | Error _ -> text "No output"

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
open Thumbnails

let getVideoFile root path =
    let path = [| root; path |] |> combinePathes  
    setContentType "video/mp4" >=> streamFile true path None None

let getPictureFile root path =
    let path = [| root; path |] |> combinePathes  
    setContentType "image/jpg" >=> streamFile false path None None

let getMusicFile root path =
    let path = [| root; path |] |> combinePathes  
    setContentType "audio/mp3" >=> streamFile true path None None

let getPicturesZipFile path =
    let path = [| path; "taufe.zip" |] |> combinePathes  
    setContentType "application/zip" >=> streamFile false path None None

let getThumbnail root path =
    let getThumbnail path next (ctx: HttpContext) =
        task {
            let path = [| root; path |] |> combinePathes  
            return! ctx.WriteStreamAsync (false, getThumbnail path, None, None)
        }
        
    setContentType "image/jpg" >=> getThumbnail path
