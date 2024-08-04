namespace Database

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type LocationPoint = {
    [<Key>]
    Id: int
    Name: string
    Latitude: double
    Longitude: double
    Timestamp: DateTime
}


