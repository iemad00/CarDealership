using CarDealership.Models.DTOs.Auth;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.User;

public interface IAuthService
{
    Task<Response> SendOtpAsync(SendOtpRequest request);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
