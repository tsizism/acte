using Microsoft.EntityFrameworkCore;
using UIPooc.Data;

namespace DBSync;

/// <summary>
/// Drops (if it exists) and re-creates the HoldingsDbBackup database
/// using the same EF Core model as HoldingsDb.
/// </summary>
public static class BackupDbManager
{
    public static async Task RecreateAsync(HoldingsDbContext backupDb, CancellationToken ct = default)
    {
        Console.WriteLine("Dropping HoldingsDbBackup (if it exists)...");
        bool dropped = await backupDb.Database.EnsureDeletedAsync(ct);
        Console.WriteLine(dropped ? "  Dropped." : "  Did not exist – skipping drop.");

        Console.WriteLine("Creating HoldingsDbBackup with current schema...");
        await backupDb.Database.EnsureCreatedAsync(ct);
        Console.WriteLine("  Done. HoldingsDbBackup is ready.");
    }
}


//Delete User --[CASCADE]--> Delete Holdings --[CASCADE]--> Delete Transactions
//Delete User --[CASCADE]-----------------------> Delete Transactions  ← conflict!
//Delete User --[NoAction]-----------------------> Delete Transactions  ← No conflict!