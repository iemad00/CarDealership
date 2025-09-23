using CarDealership.Models.DTOs.Admin;

namespace CarDealership.Services.Admin;

public interface IAdminAuthService
{
    Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
