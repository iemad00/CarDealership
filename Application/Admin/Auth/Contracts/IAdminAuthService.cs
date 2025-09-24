using CarDealership.Models.DTOs.Admin;
using CarDealership.Application.Common.Dtos;

namespace CarDealership.Services.Admin;

public interface IAdminAuthService
{
    Task<Response> SendOtpAsync(SendOtpRequest request);
    Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
