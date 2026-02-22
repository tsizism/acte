namespace UIPooc.Models
{
    public class Equity
    {
        public int EquityId { get; set; }
        public int HoldingId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public decimal Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal? CurrentPrice { get; set; }
        public Holding Holding { get; set; } = null!;
    }
}
