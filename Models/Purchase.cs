namespace CarDealership.Models;

public class Purchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int VehicleId { get; set; }
    public decimal PriceAtSale { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}


