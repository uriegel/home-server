module Requests

open Giraffe
open System.IO

open Utils

type Files  = {
    Files: string[]    
}

let getVideoList () =
    let getName (fileInfo: FileInfo) = fileInfo.Name
    let getFileNames (fileList: FileInfo[]) = Ok(
        fileList
        |> Array.map getName
        |> Array.sortWith icompare)
    let getList = getFiles >=>! getFileNames
    
    match getList "/home/uwe/Videos" with
    | Ok value -> json { Files = value }
    // TODO send error html and log error
    | Err e    -> text "No output"

