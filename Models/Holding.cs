namespace UIPooc.Models
{
    public class Holding
    {
        public int HoldingId { get; set; }
        public int UserId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public decimal Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal? CurrentPrice { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
    }
}
