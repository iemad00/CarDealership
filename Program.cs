using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarDealership.Data;
using CarDealership.Services;
using CarDealership.Extensions;
using StackExchange.Redis;
using CarDealership.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// Bind options
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32, "Jwt:Key must be at least 32 characters.");

builder.Services.AddOptions<PasscodeHashingOptions>()
    .Bind(builder.Configuration.GetSection(PasscodeHashingOptions.SectionName))
    .ValidateDataAnnotations();

// Add custom services
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasscodeHashService, PasscodeHashService>();
builder.Services.AddScoped<CarDealership.Services.User.IAuthService, CarDealership.Services.User.AuthService>();
builder.Services.AddScoped<CarDealership.Services.Admin.IAdminAuthService, CarDealership.Services.Admin.AdminAuthService>();
builder.Services.AddScoped<CarDealership.Services.Admin.IAdminManagementService, CarDealership.Services.Admin.AdminManagementService>();
// Add seeding services
builder.Services.AddScoped<CarDealership.Data.Seeds.ISeedService, CarDealership.Data.Seeds.SuperAdminSeedService>();
builder.Services.AddScoped<CarDealership.Data.Seeds.SuperAdminSeedService>();
builder.Services.AddScoped<CarDealership.Data.Seeds.DataSeeder>();

// Add background services
builder.Services.AddHostedService<StartupService>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add controllers with global route prefix
builder.Services.AddControllers(options =>
{
    options.UseGeneralRoutePrefix("api/v{version:apiVersion}");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
