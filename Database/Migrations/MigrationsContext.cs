using Database;
using Microsoft.EntityFrameworkCore;

namespace Migrations;

public class MigrationsContext : HomeServerContext
{
    public override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={HomeServerContext.DBPath}");
}
