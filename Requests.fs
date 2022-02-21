module Requests

open Giraffe
open System.IO

open Utils

type Files  = {
    Files: string[]    
}

let getVideoList () =
    let getName (fileInfo: FileInfo) = fileInfo.Name
    let getFileNames (fileList: FileInfo[]) = 
        fileList
        |> Array.map getName
        |> Array.sortWith icompare
    let getList = getFiles >=>! switchResponse getFileNames
    
    match getList "/home/uwe/Videos" with
    | Ok value -> json { Files = value }
    // TODO send error html and log error
    | Err e    -> text "No output"

let streamVideo (file: string): HttpHandler =
    let path = sprintf "/home/uwe/Videos/%s.mp4" file
    (fun () -> streamFile true path None None)()
    
     
        
        