namespace UIPooc.Models;

public enum TransactionType
{
    Buy,
    Sell,
    Watch,
}

public class Transaction
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public int HoldingId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Commission { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public User User { get; set; } = null!;
    public Holding Holding { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
}

