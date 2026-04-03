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
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketPrice { get; set; } = 0;
        public decimal? GainLoss { get; set; }
        public TransactionType LastTxnType { get; set; }
        public decimal LastTxnQuantity { get; set; }
        public decimal LastTxnPrice { get; set; }
        public DateTime LastTxnAt { get; set; }
        public decimal HoldingHigh { get; set; }
        public DateTime HoldingHighAt { get; set; }
        public decimal IndexWeight { get; set; }
        public decimal HoldingLow { get; set; }
        public DateTime HoldingLowAt { get; set; }
        public decimal? FlagMax { get; set; } = 0;
        public decimal? FlagMin { get; set; } = 0;
        public bool Flag { get; set; } = false;
        public string? FlagMessage { get; set; } = string.Empty;
        public DateTime? FlagDate { get; set; }
        public Holding Holding { get; set; } = null!;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;


        public Equity() { }

        public Equity(Equity other)
        {
            EquityId = other.EquityId;
            HoldingId = other.HoldingId;
            Currency = other.Currency;
            Market = other.Market;
            Symbol = other.Symbol;
            Keywords = other.Keywords;
            Quantity = other.Quantity;
            AverageCost = other.AverageCost;
            CurrentPrice = other.CurrentPrice;
            MarketPrice = other.MarketPrice;
            GainLoss = other.GainLoss;
            LastTxnType = other.LastTxnType;
            LastTxnQuantity = other.LastTxnQuantity;
            LastTxnPrice = other.LastTxnPrice;
            LastTxnAt = other.LastTxnAt;
            HoldingHigh = other.HoldingHigh;
            HoldingHighAt = other.HoldingHighAt;
            IndexWeight = other.IndexWeight;
            HoldingLow = other.HoldingLow;
            HoldingLowAt = other.HoldingLowAt;
            FlagMax = other.FlagMax;
            FlagMin = other.FlagMin;
            Flag = other.Flag;
            FlagMessage = other.FlagMessage;
            FlagDate = other.FlagDate;
            Holding = other.Holding;
        }

        public void CopyTo(Equity destination)
        {
            destination.EquityId = this.EquityId;
            destination.HoldingId = this.HoldingId;
            destination.Currency = this.Currency;
            destination.Market = this.Market;
            destination.Symbol = this.Symbol;
            destination.Keywords = this.Keywords;
            destination.Quantity = this.Quantity;
            destination.AverageCost = this.AverageCost;
            destination.CurrentPrice = this.CurrentPrice;
            destination.MarketPrice = this.MarketPrice;
            destination.GainLoss = this.GainLoss;
            destination.LastTxnType = this.LastTxnType;
            destination.LastTxnQuantity = this.LastTxnQuantity;
            destination.LastTxnPrice = this.LastTxnPrice;
            destination.LastTxnAt = this.LastTxnAt;
            destination.HoldingHigh = this.HoldingHigh;
            destination.HoldingHighAt = this.HoldingHighAt;
            destination.IndexWeight = this.IndexWeight;
            destination.HoldingLow = this.HoldingLow;
            destination.HoldingLowAt = this.HoldingLowAt;
            destination.FlagMax = this.FlagMax;
            destination.FlagMin = this.FlagMin;
            destination.Flag = this.Flag;
            destination.FlagMessage = this.FlagMessage;
            destination.FlagDate = this.FlagDate;
            destination.Holding = this.Holding;
        }
    }
}



/*
 * 
 using System.Reflection;

public static class ExtensionMethods
{
    public static void CopyTo(this object source, object destination)
    {
        var destinationType = destination.GetType();
        foreach (var sourceProperty in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var destinationProperty = destinationType.GetProperty(sourceProperty.Name);

            if (destinationProperty != null && destinationProperty.CanWrite && destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
            {
                destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
            }
        }
    }
}
// ...
this.CopyTo(destination);
*/