module DatabaseAccess
open Database
open System
open Database
open System.Collections.Generic

let location name latitude longitude timeStamp = 
    {
        Id = 0
        Name = name
        Latitude = latitude
        Longitude = longitude
        Timestamp = timeStamp
    }

let test () =
    use db = new HomeServerContext()

    printfn "Database path: %s" HomeServerContext.DBPath

    db.Add <| location "UweRiegel" 2.09 2.9 DateTime.Now  |> ignore
    db.Add <| location "UweRiegel" 34.09 2.49 DateTime.Now  |> ignore
    db.Add <| location "UweRiegel" 35.09 2.39 DateTime.Now  |> ignore
    db.Add <| location "UweRiegel" 33.09 1.39 DateTime.Now  |> ignore
    db.SaveChanges () |> ignore

    let locationPoints = 
        db.LocationPoints
        |> Seq.filter (fun n -> n.Name = "UweRiegel")
        |> Seq.sortByDescending (fun b -> b.Timestamp)

    locationPoints|> Seq.iter (fun p -> printfn "Id: %d, Name: %s, Age: %O" p.Id p.Name p.Timestamp)

    db.RemoveRange (locationPoints :?> IEnumerable<obj>)
    db.SaveChanges () 
    |> ignore

    // db.People
    // |> Seq.filter (fun n -> n.Id = 2)
    // |> Seq.iter (fun p -> printfn "Id: %d, Name: %s, Age: %d" p.Id p.Name p.Age)

    // printfn "jetezt"

    // db.People
    // |> Seq.iter (fun p -> printfn "Id: %d, Name: %s, Age: %d" p.Id p.Name p.Age)

    // 0 // return an integer exit code
