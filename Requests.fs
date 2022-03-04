module Requests

open FSharpRailway
open FSharpTools
open Giraffe
open Microsoft.AspNetCore.Http
open System.IO

open Utils

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

open FSharpRailway.Result    

let getMusicList root path =
    let getName (fileInfo: FileSystemInfo) = fileInfo.Name
    let getDirNames (fileList: FileSystemInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith String.icompare
    
    let checkDirectory path = if isDirectory path then Ok(path) else Error(NotADirectoryException() :> System.Exception)
    let getListFromPathParts = 
        combinePath 
        >=> checkDirectory
        >=> getFileSystemInfos 
        >=> switch getDirNames

    match getListFromPathParts [|root; path|] with
    | Ok value                                        -> json { Files = value }
    | Error e when e :? NotADirectoryException = true -> skip
    // TODO send error html and log error
    | _                                               -> text "No output"

open Giraffe

let getMusicFile root path =
    match combinePath [| root; path |] with
    | Ok path -> setContentType "audio/mp3" >=> streamFile true path None None
    | Error _ -> text "No audio file" 
    