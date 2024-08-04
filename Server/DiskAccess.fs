module DiskAccess
open FSharpTools
open Configuration
open Option

let private switchDiskOff port = 
    async {
        printfn "switching disk off..."
        let! result = Process.asyncRunCmd "/usr/sbin/uhubctl" <| (sprintf "-l 1-1 -a 0 -p %d -r 500" port)
        printfn "disk switched off\n%s" result
    } |> Async.Start

let private createTimer () =
    let createTimer port = 
        let timer = new System.Timers.Timer 300_000
        timer.AutoReset <- false
        timer.Elapsed.Add (fun _ -> switchDiskOff port) 
        timer, port

    getUsbPort ()
    |> map createTimer

let private shutdownTimer = createTimer ()

let access () =
    let access (timer: System.Timers.Timer, port) =
        async {
            printfn "accessing disk..."
            let! result = Process.asyncRunCmd "/usr/sbin/uhubctl" <| (sprintf "-l 1-1 -a 1 -p %d" port)
            do! Async.Sleep(6000)
            let! mountResult =  Process.asyncRunCmd "/usr/bin/mount" "-a"

            if timer.Enabled then
                timer.Stop ()
            timer.Start ()
            printfn "%s\n%s" result mountResult
        }

    async {
        do! 
            shutdownTimer 
            |> iterAsync access 
    }

let needed () =
    let needed (timer: System.Timers.Timer, _) = 
        if timer.Enabled then
            timer.Stop ()
            timer.Start ()

    shutdownTimer
    |> iter needed

