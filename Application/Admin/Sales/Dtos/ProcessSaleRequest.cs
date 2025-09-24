using System.ComponentModel.DataAnnotations;

namespace CarDealership.Application.Admin.Sales.Dtos;

public class ProcessSaleRequest
{
    [Required]
    public int PurchaseRequestId { get; set; }
}


