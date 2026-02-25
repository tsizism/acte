namespace UIPooc.Models
{
    public class Holding
    {
        public int HoldingId { get; set; }
        public int UserId { get; set; }
        public double Index { get; set; }
        public double? FlagMaxIndex { get; set; }
        public double? FlagMinIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CallName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public ICollection<Equity> Equities { get; set; } = new List<Equity>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<IndexHistory> IndexHistories { get; set; } = new List<IndexHistory>();
    }
}
