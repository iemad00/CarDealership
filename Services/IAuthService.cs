using CarDealership.Models.DTOs.Auth;

namespace CarDealership.Services;

public interface IAuthService
{
    Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
