using System.ComponentModel.DataAnnotations;

namespace CarDealership.Application.User.Purchases.Dtos;

public class CreatePurchaseRequestRequest
{
    [Required]
    public int VehicleId { get; set; }
}


