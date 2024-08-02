module Utils

open FSharpTools
open FSharpTools.Functional
open Option
open FSharpPlus

let getEnvironmentVariableLogged =
    let logToConsole (key, value) = printfn "Reading environment %s: %s" key value
    withInputVar String.retrieveEnvironmentVariable 
        >=> switch (sideEffect logToConsole) 
        >=> omitInputVar

let getEnvironmentVariable = memoize getEnvironmentVariableLogged



