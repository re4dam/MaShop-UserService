namespace UserService.DTOs.PublishDTO;

public class UserPublishedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
}