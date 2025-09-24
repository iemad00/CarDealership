using CarDealership.Extensions;
using CarDealership.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CarDealership.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container via extension methods
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddAuth(builder.Configuration)
    .AddApplication();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add controllers with global route prefix and uniform validation responses
builder.Services
    .AddControllers(options =>
    {
        options.UseGeneralRoutePrefix("api/v{version:apiVersion}");
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .Select(kvp => new
                {
                    field = kvp.Key,
                    errors = kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage).ToArray()
                })
                .ToArray();

            var payload = new
            {
                success = false,
                message = "Validation failed",
                data = new { errors }
            };

            return new BadRequestObjectResult(payload);
        };
    });

// Global response envelope filter for uniform API responses
builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add<ResponseEnvelopeFilter>();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseGlobalErrorHandler();

var enableHttpsRedirection = builder.Configuration.GetValue<bool>("EnableHttpsRedirection");
if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Liveness probe
app.MapGet("/health", () => "OK");

app.MapControllers();

app.Run();
