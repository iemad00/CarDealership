using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Models.DTOs.Admin;

public class CreateAdminUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AdminUserDto? User { get; set; }
}
