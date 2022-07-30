module DiskAccess
open System.Threading
open FSharpTools

let mutable private diskAccesses = 0

let register () =
    task {
        let count = Interlocked.Increment(&diskAccesses)
        if count = 1 then
            let! result = Process.runCmd "/usr/sbin/uhubctl" "-l 1-1 -a 1 -p 2"
            do! Async.Sleep(6000)
            let! mountResult = Process.runCmd "/usr/bin/mount" "-a"
            printfn "%s\n%s" result mountResult
        return count
    }
    
let unregister () =
    let count = Interlocked.Decrement(&diskAccesses)
    if count = 0 then
        async {
            do! Async.Sleep(10000)
            if diskAccesses = 0 then
                let! result = Process.runCmd "/usr/sbin/uhubctl" "-l 1-1 -a 0 -p 2 -r 500"
                printfn "%s" result
        } |> Async.Start
    count


