namespace CarDealership.Application.Admin.Customers.Dtos;

public class CustomerDto
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}


