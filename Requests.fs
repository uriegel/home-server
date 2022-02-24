module Requests

open FSharpTools
open FSharpTools.Functional
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

let getVideoList path =
    let getName (fileInfo: FileInfo) = fileInfo.Name
    let getFileNames (fileList: FileInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith String.icompare
    let getList = getFiles >=>! switchResponse getFileNames
    
    match getList path with
    | Ok value -> json { Files = value }
    // TODO send error html and log error
    | Err _    -> text "No output"

let getVideoFile path file = 
    let getMp4File file = sprintf "%s/%s.mp4" path file
    let getMkvFile file = sprintf "%s/%s.mkv" path file
    let video = getExistingFile <| getMp4File file |> Option.defaultValue (getMkvFile file)
    setContentType "video/mp4" >=> streamFile true video None None

let getMusicList root path =
    let getName (fileInfo: FileSystemInfo) = fileInfo.Name
    let getDirNames (fileList: FileSystemInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith String.icompare
    
    let checkDirectory path = if isDirectory path then Ok(path) else Err(NotADirectoryException())
    let getListFromPathParts = 
        combinePath 
        >=>! checkDirectory
        >=>! getFileSystemInfos 
        >=>! switchResponse getDirNames

    match getListFromPathParts [|root; path|] with
    | Ok value                                      -> json { Files = value }
    | Err e when e :? NotADirectoryException = true -> skip
    // TODO send error html and log error
    | _                                             -> text "No output"

let getMusicFile root path =
    match combinePath [| root; path |] with
    | Ok path -> setContentType "audio/mp3" >=> streamFile true path None None
    | Err _   -> text "No audio file" 
    