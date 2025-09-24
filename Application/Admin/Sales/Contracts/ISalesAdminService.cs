using CarDealership.Application.Admin.Sales.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public interface ISalesAdminService
{
    Task<Response<ProcessSaleData>> ProcessSaleAsync(ProcessSaleRequest request);
}


