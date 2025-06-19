using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.bc_technicians.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.bc_technicians.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;
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

// Add services to the container.

// Configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure Kebab Case Route Naming Convention
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// Configure Dependency Injection for Shared (DB-related services commented for now)
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Needs DB
// builder.Services.AddScoped<IBaseRepository<Entity>, BaseRepository<Entity>>(); // Needs DB

// Shared Bounded Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Service Request Bounded Context
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<IServiceRequestCommandService, ServiceRequestCommandService>();
builder.Services.AddScoped<IServiceRequestQueryService, ServiceRequestQueryService>();

//technicians Bounded Context
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<ITechnicianCommandService, TechnicianCommandService>();
builder.Services.AddScoped<ITechnicianQueryService, TechnicianQueryService>();
//Equipment Bounded Context
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IEquipmentCommandService, EquipmentCommandService>();
builder.Services.AddScoped<IEquipmentQueryService, EquipmentQueryService>();

// Configure Dependency Injection for Work Orders Bounded Context
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IWorkOrderCommandService, WorkOrderCommandService>();
builder.Services.AddScoped<IWorkOrderQueryService, WorkOrderQueryService>();

// Subscriptions and Payments Bounded Context
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.EnableAnnotations());

// Add Database Connection (COMMENTED - will implement in several weeks)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString is null)
    throw new Exception("Database connection string is not set.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));

var app = builder.Build();

// Database initialization (COMMENTED - will implement in several weeks)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Keep a simple endpoint for testing Osito Polar Platform!
app.MapGet("/osito", () => "¡Hola! Soy el Osito Polar Platform 🐻‍❄️")
    .WithName("GetOsito");

// Keep the weather forecast endpoint for testing
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}