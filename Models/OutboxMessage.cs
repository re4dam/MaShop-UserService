namespace UserService.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}