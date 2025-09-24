using CarDealership.Application.User.Purchases.Dtos;

namespace CarDealership.Services.User;

public interface IUserPurchaseService
{
    Task<CreatePurchaseRequestResponse> CreatePurchaseRequestAsync(int userId, CreatePurchaseRequestRequest request);
    Task<List<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(int userId);
}


