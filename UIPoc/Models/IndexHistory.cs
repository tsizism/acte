namespace UIPooc.Models;

public class IndexHistory
{
    public int IndexHistoryId { get; set; }
    public int HoldingId { get; set; }
    public decimal Index { get; set; }
    public DateOnly RecordedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? HoldingSnapshot { get; set; }
    public Holding Holding { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
}

