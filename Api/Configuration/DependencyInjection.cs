using System.Text;
using CarDealership.Data;
using CarDealership.Options;
using CarDealership.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using CarDealership.Middlewares;
using System.Text.Json;

namespace CarDealership.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
            ));

        services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddOptions<PasscodeHashingOptions>()
            .Bind(configuration.GetSection(PasscodeHashingOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.");

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPasscodeHashService, PasscodeHashService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            var payload = new
                            {
                                success = false,
                                message = "Unauthorized",
                                data = new { }
                            };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        }
                    },
                    OnForbidden = async context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            var payload = new
                            {
                                success = false,
                                message = "Forbidden",
                                data = new { }
                            };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        }
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CarDealership.Services.User.IAuthService, CarDealership.Services.User.AuthService>();
        services.AddScoped<CarDealership.Services.Admin.IAdminAuthService, CarDealership.Services.Admin.AdminAuthService>();
        services.AddScoped<CarDealership.Services.Admin.IAdminManagementService, CarDealership.Services.Admin.AdminManagementService>();
        services.AddScoped<CarDealership.Services.Admin.IVehicleAdminService, CarDealership.Services.Admin.VehicleAdminService>();
        services.AddScoped<CarDealership.Services.User.IVehicleBrowseService, CarDealership.Services.User.VehicleBrowseService>();
        services.AddScoped<CarDealership.Services.Admin.ICustomerAdminService, CarDealership.Services.Admin.CustomerAdminService>();
        services.AddScoped<CarDealership.Services.User.IUserPurchaseService, CarDealership.Services.User.UserPurchaseService>();
        services.AddScoped<CarDealership.Services.Admin.ISalesAdminService, CarDealership.Services.Admin.SalesAdminService>();

        // Seeding services
        services.AddScoped<CarDealership.Data.Seeds.ISeedService, CarDealership.Data.Seeds.SuperAdminSeedService>();
        services.AddScoped<CarDealership.Data.Seeds.SuperAdminSeedService>();
        services.AddScoped<CarDealership.Data.Seeds.DemoSeedService>();
        services.AddScoped<CarDealership.Data.Seeds.DataSeeder>();

        return services;
    }

    public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        return app;
    }
}


