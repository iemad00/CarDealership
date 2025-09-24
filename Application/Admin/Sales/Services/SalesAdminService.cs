using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Application.Admin.Sales.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public class SalesAdminService : ISalesAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SalesAdminService> _logger;

    public SalesAdminService(ApplicationDbContext context, ILogger<SalesAdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Response<ProcessSaleData>> ProcessSaleAsync(ProcessSaleRequest request)
    {
        try
        {
            var pr = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == request.PurchaseRequestId && x.Status == "Pending");
            if (pr == null)
            {
                return new Response<ProcessSaleData> { Success = false, Message = "Request not found or already processed" };
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == pr.VehicleId && v.IsActive);
            if (vehicle == null)
            {
                return new Response<ProcessSaleData> { Success = false, Message = "Vehicle not found or inactive" };
            }

            pr.Status = "Completed";

            var purchase = new Models.Purchase
            {
                UserId = pr.UserId,
                VehicleId = pr.VehicleId,
                PriceAtSale = pr.QuotedPrice,
                PurchasedAt = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);

            vehicle.IsActive = false;

            await _context.SaveChangesAsync();

            return new Response<ProcessSaleData> { Success = true, Message = "Sale processed", Data = new ProcessSaleData { PurchaseId = purchase.Id } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale");
            return new Response<ProcessSaleData> { Success = false, Message = "Failed to process sale" };
        }
    }
}


