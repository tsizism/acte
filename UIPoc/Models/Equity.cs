namespace UIPooc.Models
{
    public class Equity
    {
        public int EquityId { get; set; }
        public int HoldingId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Market { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public TransactionType LastTxnType { get; set; }
        public decimal LastTxnQuantity { get; set; }
        public decimal LastTxnPrice { get; set; }
        public DateTime LastTxnAt { get; set; }
        public decimal HoldingHigh{ get; set; }
        public decimal IndexWeight { get; set; }
        public DateTime HoldingHighAt { get; set; }
        public decimal HoldingLow { get; set; }
        public DateTime HoldingLowAt { get; set; }
        public decimal? FlagMax { get; set; } = 0;
        public decimal? FlagMin { get; set; } = 0;
        public bool Flag { get; set; } = false;
        public string? FlagMessage { get; set; } = string.Empty;
        public DateTime? FlagDate { get; set; }
        public Holding Holding { get; set; } = null!;
    }
}
