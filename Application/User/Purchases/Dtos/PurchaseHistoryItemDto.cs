namespace CarDealership.Application.User.Purchases.Dtos;

public class PurchaseHistoryItemDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public decimal QuotedPrice { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Completed
}


