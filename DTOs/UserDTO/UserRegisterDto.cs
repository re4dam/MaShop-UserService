namespace UserService.DTOs.UserDTO;

public class UserRegisterDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}