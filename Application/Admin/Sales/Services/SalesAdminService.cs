using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Application.Admin.Sales.Dtos;

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

    public async Task<ProcessSaleResponse> ProcessSaleAsync(ProcessSaleRequest request)
    {
        try
        {
            var pr = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == request.PurchaseRequestId && x.Status == "Pending");
            if (pr == null)
            {
                return new ProcessSaleResponse { Success = false, Message = "Request not found or already processed" };
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == pr.VehicleId && v.IsActive);
            if (vehicle == null)
            {
                return new ProcessSaleResponse { Success = false, Message = "Vehicle not found or inactive" };
            }

            // mark request completed and create purchase
            pr.Status = "Completed";
            var purchase = new Models.Purchase
            {
                UserId = pr.UserId,
                VehicleId = pr.VehicleId,
                PriceAtSale = pr.QuotedPrice,
                PurchasedAt = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return new ProcessSaleResponse { Success = true, Message = "Sale processed", PurchaseId = purchase.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale");
            return new ProcessSaleResponse { Success = false, Message = "Failed to process sale" };
        }
    }
}


