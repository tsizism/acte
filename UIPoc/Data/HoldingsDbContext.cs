using Microsoft.EntityFrameworkCore;
using UIPooc.Models;

namespace UIPooc.Data
{
    public class HoldingsDbContext : DbContext
    {
        public HoldingsDbContext(DbContextOptions<HoldingsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Holding> Holdings { get; set; }
        public DbSet<Equity> Equities { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUser(modelBuilder);
            ConfigureHolding(modelBuilder);
            ConfigureEquity(modelBuilder);
            ConfigureTransaction(modelBuilder);
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

        private void ConfigureHolding(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holding>(entity =>
            {
                entity.HasKey(e => e.HoldingId);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Index)
                    .IsRequired();

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                entity.HasIndex(e => e.UserId);
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

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(255);

                entity.Property(e => e.Quantity)
                    .IsRequired()
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.AverageCost)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CurrentPrice)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.HoldingId);
                entity.HasIndex(e => new { e.HoldingId, e.Symbol });
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
                    .IsRequired()
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Commission)
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
                .HasForeignKey(t => t.HoldingId);
        }
    }
}
