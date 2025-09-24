namespace CarDealership.Application.User.Purchases.Dtos;

public class PurchaseHistoryItemDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public decimal PriceAtSale { get; set; }
    public DateTime PurchasedAt { get; set; }
}


