module Requests

open Giraffe
open Utils

let show () = text "pong"

let getVideoList () =
    text "pong"
//    let getFiles = getFiles "/"