using Microsoft.EntityFrameworkCore;
using CarDealership.Data;
using CarDealership.Application.User.Purchases.Dtos;

namespace CarDealership.Services.User;

public class UserPurchaseService : IUserPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserPurchaseService> _logger;

    public UserPurchaseService(ApplicationDbContext context, ILogger<UserPurchaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreatePurchaseRequestResponse> CreatePurchaseRequestAsync(int userId, CreatePurchaseRequestRequest request)
    {
        try
        {
            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.IsActive);
            if (vehicle == null)
            {
                return new CreatePurchaseRequestResponse { Success = false, Message = "Vehicle not found or inactive" };
            }

            var exists = await _context.PurchaseRequests.AnyAsync(pr => pr.UserId == userId && pr.VehicleId == request.VehicleId && pr.Status == "Pending");
            if (exists)
            {
                return new CreatePurchaseRequestResponse { Success = false, Message = "Pending request already exists" };
            }

            var pr = new Models.PurchaseRequest
            {
                UserId = userId,
                VehicleId = request.VehicleId,
                QuotedPrice = vehicle.Price,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };
            _context.PurchaseRequests.Add(pr);
            await _context.SaveChangesAsync();

            return new CreatePurchaseRequestResponse { Success = true, Message = "Purchase request created", RequestId = pr.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase request");
            return new CreatePurchaseRequestResponse { Success = false, Message = "Failed to create request" };
        }
    }

    public async Task<List<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(int userId)
    {
        return await _context.Purchases
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new PurchaseHistoryItemDto
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                PriceAtSale = p.PriceAtSale,
                PurchasedAt = p.PurchasedAt
            })
            .ToListAsync();
    }
}


