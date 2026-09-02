using Microsoft.EntityFrameworkCore;
using UIPooc.Data;

namespace DBSync;

/// <summary>Row counts for every table in a HoldingsDbContext database.</summary>
public record TableCounts(
    string DatabaseLabel,
    int    Users,
    int    Holdings,
    int    Equities,
    int    EquityMarkets,
    int    Transactions,
    int    IndexHistories)
{
    public int Total =>
        Users + Holdings + Equities + EquityMarkets + Transactions + IndexHistories;
}

/// <summary>
/// Queries row counts from any <see cref="HoldingsDbContext"/> without loading data.
/// All six COUNT queries run in parallel for speed.
/// </summary>
public static class DbStats
{
    public static async Task<TableCounts> GetTableCountsAsync(HoldingsDbContext db,string label,CancellationToken ct = default)
    {
        return new TableCounts(
            DatabaseLabel:  label,
            Users:          await db.Users.CountAsync(ct),
            Holdings:       await db.Holdings.CountAsync(ct),
            Equities:       await db.Equities.CountAsync(ct),
            EquityMarkets:  await db.EquityMarkets.CountAsync(ct),
            Transactions:   await db.Transactions.CountAsync(ct),
            IndexHistories: await db.IndexHistories.CountAsync(ct));
    }

    public static void Print(TableCounts c)
    {
        Console.WriteLine();
        Console.WriteLine($"  Database : {c.DatabaseLabel}");
        Console.WriteLine($"  {new string('\u2500', 30)}");
        Console.WriteLine($"  {"Users",-20} {c.Users,6:N0}");
        Console.WriteLine($"  {"Holdings",-20} {c.Holdings,6:N0}");
        Console.WriteLine($"  {"Equities",-20} {c.Equities,6:N0}");
        Console.WriteLine($"  {"EquityMarkets",-20} {c.EquityMarkets,6:N0}");
        Console.WriteLine($"  {"Transactions",-20} {c.Transactions,6:N0}");
        Console.WriteLine($"  {"IndexHistories",-20} {c.IndexHistories,6:N0}");
        Console.WriteLine($"  {new string('\u2500', 30)}");
        Console.WriteLine($"  {"TOTAL",-20} {c.Total,6:N0}");
        Console.WriteLine();
    }
}
