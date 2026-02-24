namespace UIPooc.Models
{
    public class IndexHistory
    {
        public int IndexHistoryId { get; set; }
        public int HoldingId { get; set; }
        public double Index { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public Holding Holding { get; set; } = null!;
    }
}
