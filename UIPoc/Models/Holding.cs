namespace UIPooc.Models
{
    public enum HoldingType
    {
        WatchList, 
        Active,
        Sold,
        BuyPending,
    }

    public class Holding
    {
        public int HoldingId { get; set; }
        public int UserId { get; set; }
        public HoldingType Type { get; set; } = HoldingType.WatchList;
        public decimal Index { get; set; }
        public string? Currency { get; set; }
        public decimal? FlagMaxIndex { get; set; }
        public decimal? FlagMinIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CallName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public bool Flag { get; set; } = false;
        public string? FlagMessage { get; set; } = string.Empty;
        public DateTime? FlagDate { get; set; }
        public ICollection<Equity> Equities { get; set; } = new List<Equity>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<IndexHistory> IndexHistories { get; set; } = new List<IndexHistory>();
    }
}
