// CanFlyDbContext.cs
using Microsoft.EntityFrameworkCore;

public class CanFlyDbContext : DbContext
{
    public CanFlyDbContext(DbContextOptions<CanFlyDbContext> options)
        : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }
}

public class LogEntry
{
    public int LogEntryId { get; set; }
    public string PilotId { get; set; }
    public DateTime EntryDate { get; set; }
    // Add other properties based on your table structure
}
