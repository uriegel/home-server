using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Database;

public class LocationContext : DbContext
{
    public DbSet<LocationPoint> LocationPoints { get; set; }
    public string DbPath { get; }

    public LocationContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "home-server", "locations.db");
        if (new FileInfo(DbPath).DirectoryName != null && !Directory.Exists(new FileInfo(DbPath).DirectoryName))
            Directory.CreateDirectory(new FileInfo(DbPath).DirectoryName!);
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class LocationPoint
{
    public int Id { get; set; }

    public string Name { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}

