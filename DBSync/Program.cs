using DBSync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UIPooc.Data;

// DBSync.exe --recreate-backup
// DBSync.exe --copy-data

// ── Configuration ─────────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string Cs(string name) =>
    config.GetConnectionString(name)
    ?? throw new InvalidOperationException($"Connection string '{name}' missing from appsettings.json");

// ── Parse flags ───────────────────────────────────────────────────────────────
bool recreateBackup = args.Contains("--recreate-backup");
bool copyData        = args.Contains("--copy-data");

// ── Context factory ───────────────────────────────────────────────────────────
using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

HoldingsDbContext BuildCtx(string connectionString)
{
    var opts = new DbContextOptionsBuilder<HoldingsDbContext>()
        .UseSqlServer(connectionString)
        .UseLoggerFactory(loggerFactory)
        .Options;
    return new HoldingsDbContext(opts);
}

// ── --recreate-backup ─────────────────────────────────────────────────────────
if (recreateBackup)
{
    await using var backupDb = BuildCtx(Cs("BackupConnection"));
    await BackupDbManager.RecreateAsync(backupDb);
    Console.WriteLine();
}

// ── --copy-data ────────────────────────────────────────────────────────────
if (copyData)
{
    await using var sourceDb = BuildCtx(Cs("DefaultConnection"));
    await using var destDb   = BuildCtx(Cs("BackupConnection"));
    await DataCopier.CopyAsync(sourceDb, destDb);
    Console.WriteLine();
}

// ── DefaultConnection context (local HoldingsDb) ──────────────────────────────
await using var localDb = BuildCtx(Cs("DefaultConnection"));

// ── Count rows in every table ─────────────────────────────────────────────────
Console.WriteLine("Counting rows in DefaultConnection (local HoldingsDb)...");
var localCounts = await DbStats.GetTableCountsAsync(localDb, "HoldingsDb (local)");
DbStats.Print(localCounts);

