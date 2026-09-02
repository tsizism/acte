using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UIPooc.Data;
using UIPooc.Models;

namespace DBSync;

/// <summary>
/// Copies all data from a source <see cref="HoldingsDbContext"/> (DefaultConnection)
/// into a destination <see cref="HoldingsDbContext"/> (BackupConnection).
/// Tables are cleared and re-populated in FK-safe order.
/// Original primary-key values are preserved via IDENTITY_INSERT.
/// </summary>
public static class DataCopier
{
    private const int BatchSize = 500;

    public static async Task CopyAsync(
        HoldingsDbContext source,
        HoldingsDbContext dest,
        CancellationToken ct = default)
    {
        Console.WriteLine("Starting data copy: DefaultConnection → BackupConnection");
        Console.WriteLine();

        // ── 1. Read all data from source (no tracking needed) ────────────────
        Console.WriteLine("  Reading source data...");

        var users          = await source.Users         .AsNoTracking().ToListAsync(ct);
        var holdings       = await source.Holdings      .AsNoTracking().ToListAsync(ct);
        var equityMarkets  = await source.EquityMarkets .AsNoTracking().ToListAsync(ct);
        var equities       = await source.Equities      .AsNoTracking().ToListAsync(ct);
        var transactions   = await source.Transactions  .AsNoTracking().ToListAsync(ct);
        var indexHistories = await source.IndexHistories.AsNoTracking().ToListAsync(ct);

        Console.WriteLine($"    Users:          {users.Count,6:N0}");
        Console.WriteLine($"    Holdings:       {holdings.Count,6:N0}");
        Console.WriteLine($"    EquityMarkets:  {equityMarkets.Count,6:N0}");
        Console.WriteLine($"    Equities:       {equities.Count,6:N0}");
        Console.WriteLine($"    Transactions:   {transactions.Count,6:N0}");
        Console.WriteLine($"    IndexHistories: {indexHistories.Count,6:N0}");
        Console.WriteLine();

        // ── 2. Clear destination in reverse FK order ──────────────────────────
        Console.WriteLine("  Clearing destination tables...");
        await ClearAllAsync(dest, ct);
        Console.WriteLine("  Done clearing.");
        Console.WriteLine();

        // ── 3. Insert in FK-safe order, preserving PKs via IDENTITY_INSERT ────
        // Capture the connection string once, before any SetDbConnection calls
        // corrupt EF's internal _connectionString field.
        var destConnString = dest.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Destination context has no connection string.");

        // Parents must be inserted before their children.
        await InsertWithIdentityAsync(dest, destConnString, "Users", users,
            chunk => dest.Users.AddRange(chunk), ct);

        await InsertWithIdentityAsync(dest, destConnString, "Holdings", holdings,
            chunk => dest.Holdings.AddRange(chunk), ct);

        await InsertWithIdentityAsync(dest, destConnString, "EquityMarkets", equityMarkets,
            chunk => dest.EquityMarkets.AddRange(chunk), ct);

        await InsertWithIdentityAsync(dest, destConnString, "Equities", equities, chunk => dest.Equities.AddRange(chunk), ct);

        await InsertWithIdentityAsync(dest, destConnString, "Transactions", transactions,
            chunk => dest.Transactions.AddRange(chunk), ct);

        await InsertWithIdentityAsync(dest, destConnString, "IndexHistories", indexHistories,
            chunk => dest.IndexHistories.AddRange(chunk), ct);

        Console.WriteLine();
        Console.WriteLine("Data copy complete.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Deletes all rows from every table in reverse FK dependency order
    /// so FK constraints are never violated.
    /// </summary>
    private static async Task ClearAllAsync(HoldingsDbContext db, CancellationToken ct)
    {
        // Reverse of insert order: children first, then parents
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [IndexHistories]", ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Transactions]",   ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Equities]",       ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [EquityMarkets]",  ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Holdings]",       ct);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM [Users]",          ct);
    }

    /// <summary>
    /// Inserts <paramref name="rows"/> into <paramref name="tableName"/> in batches,
    /// with IDENTITY_INSERT ON so source PKs are preserved.
    /// </summary>
    private static async Task InsertWithIdentityAsync<T>(
        HoldingsDbContext db,
        string connString,
        string tableName,
        List<T> rows,
        Action<List<T>> addRange,
        CancellationToken ct) where T : class
    {
        Console.Write($"  Copying {tableName,-16} ({rows.Count,6:N0} rows)...");

        if (rows.Count == 0)
        {
            Console.WriteLine(" skipped (empty).");
            return;
        }

        // Process in batches to avoid huge single transactions
        for (int offset = 0; offset < rows.Count; offset += BatchSize)
        {
            var batch = rows.GetRange(offset, Math.Min(BatchSize, rows.Count - offset));

            // Open a raw ADO.NET connection so we can toggle IDENTITY_INSERT,
            // then let EF use the same connection for SaveChanges.
            await using var conn = new SqlConnection(connString);
            await conn.OpenAsync(ct);

            await using var identityOn = conn.CreateCommand();
            identityOn.CommandText = $"SET IDENTITY_INSERT [dbo].[{tableName}] ON";
            await identityOn.ExecuteNonQueryAsync(ct);

            // Give EF the open connection for this batch
            db.Database.SetDbConnection(conn);

            try
            {
                // Detach any tracked entities to avoid key conflicts between batches
                db.ChangeTracker.Clear();

                addRange(batch);
                await db.SaveChangesAsync(ct);
                db.ChangeTracker.Clear();
            }
            finally
            {
                await using var identityOff = conn.CreateCommand();
                identityOff.CommandText = $"SET IDENTITY_INSERT [dbo].[{tableName}] OFF";
                await identityOff.ExecuteNonQueryAsync(ct);

                // Return EF to its own connection management
                db.Database.SetDbConnection(null);
            }
        }

        Console.WriteLine(" done.");
    }
}
