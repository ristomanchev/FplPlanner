using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ProektIntegrirani.Repository;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
    .AddRepositories()
    .AddApplicationServices()
    .AddMappers()
    .AddFplIntegration(builder.Configuration)
    .AddMessaging(builder.Configuration)
    .AddEmail(builder.Configuration)
    .AddInboundProcessing()
    .AddExternalApiRateLimiting();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Enums travel as names ("Midfielder") instead of numbers, both in requests and responses.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // API UI at /scalar
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Order matters: the middleware identifies the ApiClient that the rate limiter then partitions by.
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseRateLimiter();

app.MapControllers();

app.Run();
