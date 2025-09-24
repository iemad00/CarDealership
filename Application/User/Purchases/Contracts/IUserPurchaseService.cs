using CarDealership.Application.User.Purchases.Dtos;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.User;

public interface IUserPurchaseService
{
    Task<Response<CreatePurchaseRequestData>> CreatePurchaseRequestAsync(int userId, CreatePurchaseRequestRequest request);
    Task<List<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(int userId);
}


