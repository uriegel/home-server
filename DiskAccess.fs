module DiskAccess
open FSharpTools

let private switchDiskOff _ = 
    async {
        printfn "switching disk off..."
        let! result = Process.runCmd "/usr/sbin/uhubctl" "-l 1-1 -a 0 -p 2 -r 500"
        printfn "disk switched off\n%s" result
    } |> Async.Start

let private createTimer () =
    let timer = new System.Timers.Timer 300_000
    timer.AutoReset <- false
    timer.Elapsed.Add switchDiskOff
    timer

let private shutdownTimer = createTimer ()

let access () =
    async {
        printfn "accessing disk..."
        let! result = Process.runCmd "/usr/sbin/uhubctl" "-l 1-1 -a 1 -p 2"
        do! Async.Sleep(6000)
        let! mountResult =  Process.runCmd "/usr/bin/mount" "-a"

        if shutdownTimer.Enabled then
            shutdownTimer.Stop ()
        shutdownTimer.Start ()
        printfn "%s\n%s" result mountResult
    }

let needed () =
    if shutdownTimer.Enabled then
        shutdownTimer.Stop ()
        shutdownTimer.Start ()

