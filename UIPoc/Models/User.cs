namespace UIPooc.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Holding> Holdings { get; set; } = new List<Holding>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public bool IsDeleted { get; set; } = false;
}

