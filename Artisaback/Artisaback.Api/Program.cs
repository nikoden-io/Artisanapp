using System.Text;
using Artisaback.Api.Middleware;
using Artisaback.Application.Interfaces;
using Artisaback.Application.Services;
using Artisaback.Data.DbContext;
using Artisaback.Data.Repositories;
using Artisaback.Domain.Entities;
using Artisaback.Domain.IRepositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
var mongoConfig = builder.Configuration.GetSection("MongoDb");
var connectionString = mongoConfig["ConnectionString"];
var database = mongoConfig["Database"];

builder.Services.AddDbContext<MongoDbContext>(options =>
    options.UseMongoDB(connectionString, database));

// Configure JWT authentication.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = "Artisaback",
            ValidAudience = "ArtisabackUsers",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("YourLongerSuperSecretKey1234567890")),
            ValidateLifetime = true
        };
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ArtisanOnly", policy => policy.RequireRole("Artisan"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("DeliveryOnly", policy => policy.RequireRole("DeliveryPartner"));
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["SeedAdmin:Email"];
    var adminPassword = config["SeedAdmin:Password"];

    var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = "Admin",
            RefreshToken = "seed", // ou générer un token
            RefreshTokenExpiryTime = DateTime.UtcNow.AddYears(1)
        };
        await userRepository.CreateAsync(admin);
    }
}

app.Run();

public partial class Program
{
}