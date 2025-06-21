using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Analytics.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Analytics.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.bc_technicians.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Configuration;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.External.Services;
using OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.EquipmentManagement.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.ServiceRequests.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using OsitoPolarPlatform.API.WorkOrders.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.WorkOrders.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Repositories;

var builder = WebApplication.CreateBuilder(args);

var isProduction = builder.Environment.IsProduction() || 
                  Environment.GetEnvironmentVariable("RENDER") != null ||
                  Environment.GetEnvironmentVariable("PORT") != null;


if (isProduction)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($" Production mode - Port: {port}");
}
else
{
    Console.WriteLine("🛠️  Development mode - http://localhost:5128");
}

// Add services to the container.

// Configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure Kebab Case Route Naming Convention
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// Configure CORS
builder.Services.AddCors(options =>
{
    if (isProduction)
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    }
    else
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",   
                    "http://localhost:3001", 
                    "http://localhost:5173",   
                    "http://127.0.0.1:3000",
                    "https://localhost:5173")  
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    }
});

// Shared Bounded Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Service Request Bounded Context
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<IServiceRequestCommandService, ServiceRequestCommandService>();
builder.Services.AddScoped<IServiceRequestQueryService, ServiceRequestQueryService>();

// Analytics Bounded Context
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsCommandService, AnalyticsCommandService>();
builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

// Technicians Bounded Context
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<ITechnicianCommandService, TechnicianCommandService>();
builder.Services.AddScoped<ITechnicianQueryService, TechnicianQueryService>();

// Equipment Bounded Context
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IEquipmentCommandService, EquipmentCommandService>();
builder.Services.AddScoped<IEquipmentQueryService, EquipmentQueryService>();

// Work Orders Bounded Context
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IWorkOrderCommandService, WorkOrderCommandService>();
builder.Services.AddScoped<IWorkOrderQueryService, WorkOrderQueryService>();

// Subscriptions and Payments Bounded Context
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();

// Stripe Configuration
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

// Payment Services
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IStripeService, StripeService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.EnableAnnotations());

// Add Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString is null)
    throw new Exception("Database connection string is not set.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OsitoPolar API V1");
    if (isProduction)
    {
        c.RoutePrefix = string.Empty; 
    }
});


if (!isProduction)
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthorization();
app.MapControllers();


Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Connection String configured: {!string.IsNullOrEmpty(connectionString)}");

if (isProduction)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    Console.WriteLine($"📍 Swagger: http://0.0.0.0:{port}");
    Console.WriteLine($"📍 Equipment API: http://0.0.0.0:{port}/api/v1/equipments");
}
else
{
    Console.WriteLine($"📍 Swagger: http://localhost:5000/swagger");
    Console.WriteLine($"📍 Equipment API: http://localhost:5000/api/v1/equipments");
}

app.Run();