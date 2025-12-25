using PhotoCoop.Application.Bookings;
using PhotoCoop.Application.Matching;
using PhotoCoop.Application.Users;
using PhotoCoop.Application.Admins;
using PhotoCoop.Domain.Bookings;
using PhotoCoop.Domain.Users;
using PhotoCoop.Infrastructure.Cosmos;
using PhotoCoop.Infrastructure.Cosmos.Bookings;
using PhotoCoop.Infrastructure.Cosmos.Users;

var builder = WebApplication.CreateBuilder(args);

// Configure CosmosDbOptions from configuration
builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection(CosmosDbOptions.SectionName));

// Register CosmosClientFactory as a singleton
builder.Services.AddSingleton<CosmosClientFactory>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPhotographerMatchingService, PhotographerMatchingService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Add controllers and configure JSON options
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Optional: Add OpenAPI/Swagger if needed
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
