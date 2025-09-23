using CarDealership.Application.Admin.Customers.Dtos;

namespace CarDealership.Services.Admin;

public interface ICustomerAdminService
{
    Task<List<CustomerDto>> GetAllAsync();
}


