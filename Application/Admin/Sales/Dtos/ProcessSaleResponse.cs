namespace CarDealership.Application.Admin.Sales.Dtos;

public class ProcessSaleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? PurchaseId { get; set; }
}


