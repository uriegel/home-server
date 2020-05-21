open System
open System.Runtime.InteropServices
open System.Threading
open Session
open UwebProxy
open System.IO

type Videos = {
    Files: string array
}

[<UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Auto)>]
type Callback = delegate of int -> unit

[<DllImport("libc", SetLastError = true)>]
extern int signal(int pid, Callback handleSignal)

let stopping = new ManualResetEvent false

let videoRequest = Static.useStatic "/media/uwe/Volume" "/video"
let homePageRequest = Static.useStatic "/home/uwe/webroot" "/" 
let filmRequest = Static.useStatic "home/uwe/" "/Starbuzz"
let favicon = Static.useFavicon "/home/uwe/webroot/Uwe.jpg"
let fritzProxy = useReverseProxyByHost "fritz.uriegel.de" "http://fritz.box"
let testProxy = useReverseProxyByHost "familie.uriegel.de" "http://queensbridge"

let videoFiles (requestSession: RequestSession) =
    async {
        let request = requestSession.Query.Value
        match requestSession.Query.Value.Request with
        | "videos" ->        

            let dir = DirectoryInfo("/media/uwe/Volume/video")
            let files = 
                dir.GetFiles()
                |> Array.map(fun n -> n.Name)
            let videos = {
                Files = files
            }

            do! requestSession.AsyncSendJson (videos :> obj)
            return true
        | _ -> return false
    }


let configuration = Configuration.create {
    Configuration.createEmpty() with 
        DomainName = "uriegel.de"
        UseLetsEncrypt = true
        Requests = [ fritzProxy; testProxy; videoFiles; videoRequest; filmRequest; homePageRequest; favicon ]
}

try 
    let server = Server.create configuration 

    printfn "Version 2"

    let handleTerm i = 
        server.stop ()
        stopping.Set () |> ignore

    let handleTermDelegate = Callback (handleTerm)
    let res = signal (15, handleTermDelegate) 

    server.start ()
    stopping.WaitOne () |> ignore
with
    ex -> printfn "WebServer konnte nicht gestartet werden: %O" ex
