namespace UIPooc.Models;

public class AIKnowledgeItem
{
    public int AIKnowledgeItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? QuestionsToAnswer { get; set; }
    public string? AKAs { get; set; }
    public string? Scope { get; set; }
    public string? Source { get; set; }
    public string? Chunks { get; set; }
    public int EstimatedTokens { get; set; }
    public int EstimatedDurationSec { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
