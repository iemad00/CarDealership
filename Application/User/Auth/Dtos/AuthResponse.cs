namespace CarDealership.Models.DTOs.User;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? LoginToken { get; set; }
    public bool FirstLogin { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
