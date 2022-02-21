module Requests

open Giraffe
open Microsoft.AspNetCore.Http
open System.IO

open Configuration
open Utils

type Files  = {
    Files: string[]    
}

let setContentType contentType (next: HttpFunc) (ctx: HttpContext) =
    ctx.SetHttpHeader("Content-Type", contentType)
    next ctx

let getVideoList path =
    let getName (fileInfo: FileInfo) = fileInfo.Name
    let getFileNames (fileList: FileInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith icompare
    let getList = getFiles >=>! switchResponse getFileNames
    
    match getList path with
    | Ok value -> json { Files = value }
    // TODO send error html and log error
    | Err e    -> text "No output"

let getVideoFile path file = 
    let getMp4File file = sprintf "%s/%s.mp4" path file
    let getMkvFile file = sprintf "%s/%s.mkv" path file
    let video = getExistingFile <| getMp4File file |> Option.defaultValue (getMkvFile file)
    setContentType "video/mp4" >=> streamFile true video None None

