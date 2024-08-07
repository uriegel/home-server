namespace Database

open System
open Microsoft.EntityFrameworkCore

type HomeServerContext() =
    inherit DbContext()

    [<DefaultValue>]
    val mutable locationPoints: DbSet<LocationPoint>
    member this.LocationPoints
        with get() = this.locationPoints
        and set v = this.locationPoints <- v

    static member DBPath
        with get() = 
            System.IO.Path.Join(Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData, 
                                "home-server", 
                                // TODO ensurePath exists
                                "database.db")

    override this.OnConfiguring(optionsBuilder: DbContextOptionsBuilder) =
        optionsBuilder.UseSqlite(sprintf "Data Source=%s" HomeServerContext.DBPath) |> ignore
