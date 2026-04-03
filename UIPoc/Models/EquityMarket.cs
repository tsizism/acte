namespace UIPooc.Models
{
    //public string? Exchange { get; set; }
    //public string Market { get; set; } = string.Empty;
    //public string? CompanyName { get; set; }
    //public decimal? PERatio { get; set; }
    //public decimal? DividendYield { get; set; }
    //public decimal? EPS { get; set; }
/*
                 entity.Property(e => e.Market)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(255);

                entity.Property(e => e.PERatio)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.DividendYield)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.EPS)
                    .HasColumnType("decimal(18,4)");

                entity.Property(e => e.Exchange)
                    .HasMaxLength(50);

                entity.HasIndex(e => new { e.Symbol, e.Market })
                    .IsUnique();

                entity.HasIndex(e => e.Market);
*/ 


    public class EquityMarket
    {
        public int EquityMarketId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayLow { get; set; }
        public long Volume { get; set; }
        public decimal? MarketCap { get; set; }
        public decimal? Week52High { get; set; }
        public decimal? Week52Low { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime? LastTradeTime { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

