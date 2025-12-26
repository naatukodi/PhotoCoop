using PhotoCoop.Application.Bookings;
using PhotoCoop.Application.Matching;
using PhotoCoop.Application.Users;
using PhotoCoop.Application.Admins;
using PhotoCoop.Application.Payments;
using PhotoCoop.Application.Memberships;
using PhotoCoop.Domain.Bookings;
using PhotoCoop.Domain.Users;
using PhotoCoop.Domain.Payments;
using PhotoCoop.Infrastructure.Cosmos;
using PhotoCoop.Infrastructure.Cosmos.Bookings;
using PhotoCoop.Infrastructure.Cosmos.Users;
using PhotoCoop.Infrastructure.Cosmos.Payments;
using PhotoCoop.Infrastructure.Razorpay;

var builder = WebApplication.CreateBuilder(args);

// Configure CosmosDbOptions from configuration
builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection(CosmosDbOptions.SectionName));

// Razorpay configuration
builder.Services.Configure<PhotoCoop.Infrastructure.Razorpay.RazorpayOptions>(
    builder.Configuration.GetSection(PhotoCoop.Infrastructure.Razorpay.RazorpayOptions.SectionName));

// Register CosmosClientFactory as a singleton
builder.Services.AddSingleton<CosmosClientFactory>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentAttemptRepository, PaymentAttemptRepository>();

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPhotographerMatchingService, PhotographerMatchingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// HTTP clients / gateways
builder.Services.AddHttpClient<IRazorpayClient, RazorpayClient>();

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
