module DatabaseAccess
open Database
open System
open Database
open System.Collections.Generic

let test () =
    use db = new HomeServerContext()

    printfn "Database path: %s" HomeServerContext.DBPath

    // TODO auto created value ID
    db.Add { Id = 0; Name = "UweRiegel"; Latitude = 2.09; Longitude = 2.9; Timestamp = DateTime.Now } |> ignore
    db.Add { Id = 0; Name = "UweRiegel"; Latitude = 34.09; Longitude = 2.49; Timestamp = DateTime.Now } |> ignore
    db.Add { Id = 0; Name = "UweRiegel"; Latitude = 35.09; Longitude = 2.39; Timestamp = DateTime.Now } |> ignore
    db.Add { Id = 0; Name = "TinaRiegel"; Latitude = 33.09; Longitude = 1.39; Timestamp = DateTime.Now } |> ignore
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
