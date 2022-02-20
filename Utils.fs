module Utils

open System.Security.Cryptography.X509Certificates

module OptionFish = 
    let (>=>) switch1 switch2 x =
        match switch1 x with
        | Some s -> switch2 s
        | None -> None

let switch f x =
    f x |> Some 

let tee f x =
    f x |> ignore
    x       

let OptionFrom2Options a b = 
    match a, b with
    | Some a, Some b -> Some (a, b)
    | _ -> None

let withInputVar switch x = 
    match switch x with
    | Some s -> Some(x, s)
    | None -> None

let omitInputVar (_, b)  = Some(b)

let exceptionToOption func =
    try
        match func () with
        | res when res <> null -> Some(res) 
        | _ -> None
    with
    | _ -> None

let parseInt (str: string) = 
    match System.Int32.TryParse str with
    | true,int -> Some int
    | _ -> None

let getEnvironmentVariable key =
    exceptionToOption (fun () -> System.Environment.GetEnvironmentVariable key)  

open OptionFish

let getEnvironmentVariableLogged =
    let logToConsole (key, value) = printfn "Reading environment %s: %s" key value
    (withInputVar getEnvironmentVariable) 
        >=> switch (tee logToConsole) 
        >=> omitInputVar

let getCertificateFromFile certPath keyPath =
    exceptionToOption (fun () -> X509Certificate2.CreateFromPemFile (certPath, keyPath))

let pathCombine subPath path =
    exceptionToOption (fun () -> System.IO.Path.Combine (path, subPath))



