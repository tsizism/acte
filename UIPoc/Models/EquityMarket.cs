namespace UIPooc.Models
{
    public class EquityMarket
    {
        public int EquityMarketId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Market { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
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
        public decimal? PERatio { get; set; }
        public decimal? DividendYield { get; set; }
        public decimal? EPS { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime? LastTradeTime { get; set; }
        public string? Exchange { get; set; }
    }
}
