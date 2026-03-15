using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UIPooc.Models;

namespace UIPooc.Data
{
    public class HoldingsDbContext : DbContext
    {
        private static readonly string[] SpecificColorNames = new string[]
            {
        "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Pink", "Cyan",
        "Magenta", "Lime", "Teal", "Navy", "Maroon", "Olive", "Gray", "Black"
            };

        public HoldingsDbContext(DbContextOptions<HoldingsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Holding> Holdings { get; set; }
        public DbSet<Equity> Equities { get; set; }
        public DbSet<EquityMarket> EquityMarkets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<IndexHistory> IndexHistories { get; set; }

        //protected override void Seed(ModelBuilder modelBuilder)
        //{
        //    // Add synchronous seeding logic here
        //}

        //protected override Task UseAsyncSeeding(ModelBuilder modelBuilder, CancellationToken cancellationToken = default)
        //{
        //    // Add asynchronous seeding logic here (e.g., making an external API call)
        //    // Ensure this mirrors the sync version's logic where possible for tooling compatibility
        //    // This is often better handled by creating a separate DbContext instance within the method
        //    return Task.CompletedTask;
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUser(modelBuilder);
            ConfigureHolding(modelBuilder);
            ConfigureEquity(modelBuilder);
            ConfigureEquityMarket(modelBuilder);
            ConfigureTransaction(modelBuilder);
            ConfigureIndexHistory(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Phone)
                    .HasMaxLength(64);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasMany(e => e.Holdings)
                    .WithOne(h => h.User)
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Transactions)
                    .WithOne(t => t.User)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public async Task SeedDefaultUserAsync()
        {
            try
            {
                // Check if any users exist
                if (!await Users.AnyAsync())
                {
                    var defaultUser = new UIPooc.Models.User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Phone = "555-0100",
                        CreatedAt = DateTime.UtcNow
                    };

                    Users.Add(defaultUser);
                    await SaveChangesAsync();

                    //logger.LogInformation("Default user 'admin' created successfully.");
                }
                else
                {
                    //logger.LogInformation("Users already exist. Skipping default user seed.");
                }
            }
            catch (Exception)
            {
                //logger.LogError(ex, "Error seeding default user.");
            }
        }

        private void ConfigureHolding(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holding>(entity =>
            {
                entity.HasKey(e => e.HoldingId);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Index)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasMaxLength(5);

                entity.Property(e => e.FlagMaxIndex)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.FlagMinIndex)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CallName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.HasIndex(p => p.Name).IsUnique();

            });
        }

        private void ConfigureEquity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equity>(entity =>
            {
                entity.HasKey(e => e.EquityId);

                entity.Property(e => e.HoldingId)
                    .IsRequired();

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Market)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(255);

                entity.Property(e => e.Keywords)
                    .HasMaxLength(255);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.AverageCost)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CurrentPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LastTxnType)
                    .IsRequired();

                entity.Property(e => e.LastTxnPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LastTxnQuantity)
                    .IsRequired()
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.LastTxnAt)
                    .IsRequired();

                entity.Property(e => e.HoldingHigh)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.IndexWeight)
                    .HasColumnType("decimal(4,2)");

                entity.Property(e => e.HoldingHighAt)
                        .IsRequired();

                entity.Property(e => e.HoldingLow)
                    .IsRequired() 
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.HoldingLowAt)
                    .IsRequired();

                entity.Property(e => e.FlagMax)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.FlagMin)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.HoldingId);
                entity.HasIndex(e => new { e.HoldingId, e.Symbol });
            });
        }

        private void ConfigureEquityMarket(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquityMarket>(entity =>
            {
                entity.HasKey(e => e.EquityMarketId);

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Market)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(255);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.CurrentPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PreviousClose)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.OpenPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DayHigh)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DayLow)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Volume)
                    .IsRequired();

                entity.Property(e => e.MarketCap)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Week52High)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Week52Low)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PERatio)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.DividendYield)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.EPS)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                entity.Property(e => e.LastTradeTime);

                entity.Property(e => e.Exchange)
                    .HasMaxLength(50);

                entity.HasIndex(e => new { e.Symbol, e.Market })
                    .IsUnique();

                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.Market);
                entity.HasIndex(e => e.LastUpdated);
            });
        }

        private void ConfigureTransaction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.HoldingId)
                    .IsRequired();

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Type)
                    .IsRequired();

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Commission)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransactionDate)
                    .IsRequired();

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.HoldingId);
            });
        }

        private void ConfigureIndexHistory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexHistory>(entity =>
            {
                entity.HasKey(e => e.IndexHistoryId);

                entity.Property(e => e.HoldingId)
                    .IsRequired();

                entity.Property(e => e.Index)
                    .IsRequired();

                entity.Property(e => e.RecordedAt)
                    .IsRequired();

                entity.Property(e => e.HoldingSnapshot)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.HasIndex(e => e.HoldingId);
                entity.HasIndex(e => e.RecordedAt);
                entity.HasIndex(e => new { e.HoldingId, e.RecordedAt });
            });
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holding>()
                .HasMany(h => h.Equities)
                .WithOne(e => e.Holding)
                .HasForeignKey(e => e.HoldingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Holding>()
                .HasMany(h => h.Transactions)
                .WithOne(t => t.Holding)
                .HasForeignKey(t => t.HoldingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Holding>()
                .HasMany(h => h.IndexHistories)
                .WithOne(i => i.Holding)
                .HasForeignKey(i => i.HoldingId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public async Task<Holding> CreateHolding(Holding holding)
        {
            holding.FlagMaxIndex = 0;
            holding.FlagMinIndex = 0;
            holding.CallName = SpecificColorNames[Random.Shared.Next(SpecificColorNames.Length)];
            Holdings.Add(holding);
            await SaveChangesAsync();
            return holding;
        }
    }
}
