using CarDealership.Extensions;
using CarDealership.Configuration;

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
