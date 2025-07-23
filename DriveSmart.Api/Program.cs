using System.Text;
using DriveSmart.Application.Services;
using DriveSmart.Persistence.Data;
using DriveSmart.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://www.drivia.it")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidAudience = config["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// Register DB and Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserRefreshTokenRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TheoryService>();
builder.Services.AddScoped<QuizService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed data BEFORE the app starts handling requests
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedData.Initialize(db);
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Seeding database failed.");
    // Optionally, rethrow or handle the error as you see fit
}

app.UseHttpsRedirection();

app.UseAuthentication();

// Use the CORS policy
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
