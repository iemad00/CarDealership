using System.ComponentModel.DataAnnotations;

namespace CarDealership.Application.User.Purchases.Dtos;

public class CreatePurchaseRequestRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }
}


