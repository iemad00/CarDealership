using CarDealership.Application.Admin.Sales.Dtos;

namespace CarDealership.Services.Admin;

public interface ISalesAdminService
{
    Task<ProcessSaleResponse> ProcessSaleAsync(ProcessSaleRequest request);
}


