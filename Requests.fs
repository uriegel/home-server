module Requests

open FSharpRailway
open FSharpTools
open Giraffe
open Microsoft.AspNetCore.Http
open System.IO

open Directory

type Files  = {
    Files: string[]    
}

type NotADirectoryException() = inherit System.Exception()

let setContentType contentType (next: HttpFunc) (ctx: HttpContext) =
    ctx.SetHttpHeader("Content-Type", contentType)
    next ctx

open FSharpRailway.Result

let getVideoList path =
    let getName (fileInfo: FileInfo) = fileInfo.Name
    let getFileNames (fileList: FileInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith String.icompare
    let getList = getFiles >=> switch getFileNames
    
    match getList path with
    | Ok value -> json { Files = value }
    // TODO send error html and log error
    | Error _  -> text "No output"

open Giraffe

let getVideoFile path file = 
    let getMp4File file = sprintf "%s/%s.mp4" path file
    let getMkvFile file = sprintf "%s/%s.mkv" path file
    let video = getExistingFile <| getMp4File file |> Option.defaultValue (getMkvFile file)
    setContentType "video/mp4" >=> streamFile true video None None

let getLetsEncryptToken token = 
    let makeTokenFileName tokenFile = 
        let combineWithToken = attachSubPath tokenFile
        Configuration.getLetsEncryptPath >> Option.map combineWithToken
    let makeTokenPath = makeTokenFileName "token" 

    let path = makeTokenPath () |> Option.defaultValue ""
    setContentType "text/plain" >=> streamFile false path None None

open FSharpRailway.Result    
open GiraffeTools

let getFileList root path =
    let getName (fileInfo: FileSystemInfo) = fileInfo.Name
    let getDirNames (fileList: FileSystemInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith String.icompare
    
    let checkDirectory path = if existsDirectory path then Ok(path) else Error(NotADirectoryException() :> System.Exception)
    let getListFromPathParts = 
        combinePathes 
        >> checkDirectory
        >=> getFileSystemInfos 
        >=> switch getDirNames

    match getListFromPathParts [|root; path|] with
    | Ok value                                        -> json { Files = value }
    | Error e when e :? NotADirectoryException = true -> skip
    // TODO send error html and log error
    | Error _                                         -> text "No output"

open System.Diagnostics
open FSharpRailway.Option
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

let getPictureFile root path =
    let path = [| root; path |] |> Directory.combinePathes  
    setContentType "image/jpg" >=> streamFile false path None None

let getMusicFile root path =
    let path = [| root; path |] |> Directory.combinePathes  
    setContentType "audio/mp3" >=> streamFile true path None None

let getPicturesZipFile path =
    let path = [| path; "taufe.zip" |] |> Directory.combinePathes  
    setContentType "application/zip" >=> streamFile false path None None
