module Utils

module OptionFish = 
    let (>=>) switch1 switch2 x =
        match switch1 x with
        | Some s -> switch2 s
        | None -> None

let exceptionToOption func =
    try
        let res = func ()
        if res <> null then
            Some(res) 
        else 
            None
    with
    | _ -> None

let parseInt (str: string) = 
    match System.Int32.TryParse str with
    | true,int -> Some int
    | _ -> None

let getEnvironmentVariable key =
    exceptionToOption (fun () -> System.Environment.GetEnvironmentVariable key)

// let httpsPort () = Utils.getEnvironmentVariable "SERVER_PORT"
// let httpsPort2 () = Utils.getEnvironmentVariable "SERVER_PORTchen"
// let httpsPort5 () = Utils.getEnvironmentVariable null

// let aff1 = httpsPort()
// let aff2 = httpsPort2()
// let aff3 = httpsPort5()

// let a = aff2.IsNone
// let a3 = aff3.IsNone


