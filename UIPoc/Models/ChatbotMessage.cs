namespace UIPooc.Models;

public class ChatbotMessage
{
    public int ChatbotMessageId { get; set; }
    public int UserId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public string Model { get; set; } = string.Empty;
    public int SessionSQN { get; set; }
    public string Content { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int AnswerTokens { get; set; }
    public int DurationMs { get; set; }
    public string? Error { get; set; }
}
