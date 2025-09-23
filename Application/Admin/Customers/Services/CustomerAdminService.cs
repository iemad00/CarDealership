using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Application.Admin.Customers.Dtos;

namespace CarDealership.Services.Admin;

public class CustomerAdminService : ICustomerAdminService
{
    private readonly ApplicationDbContext _context;

    public CustomerAdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new CustomerDto
            {
                Id = u.Id,
                Phone = u.Phone,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            })
            .ToListAsync();
    }
}


