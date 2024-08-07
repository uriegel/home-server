namespace Superfit

module Requests =
    let login (input: LoginInput) = 
        printfn "Registered superfit user: %s" input.AndroidId
        { Registered = true }
