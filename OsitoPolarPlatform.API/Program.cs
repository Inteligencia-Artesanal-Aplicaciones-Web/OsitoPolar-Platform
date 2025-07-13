using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OsitoPolarPlatform.API.Analytics.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Analytics.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Infrastructure.Persistence.EFC.Repositories;
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
using OsitoPolarPlatform.API.IAM.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.IAM.Application.Internal.OutboundServices;
using OsitoPolarPlatform.API.IAM.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.IAM.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using OsitoPolarPlatform.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using OsitoPolarPlatform.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using OsitoPolarPlatform.API.IAM.Infrastructure.Tokens.JWT.Services;
using OsitoPolarPlatform.API.IAM.Interfaces.ACL;
using OsitoPolarPlatform.API.IAM.Interfaces.ACL.Services;
using OsitoPolarPlatform.API.Profiles.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Profiles.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Profiles.Domain.Services;
using OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.ServiceRequests.Application.Internal.QueryServices;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Services;
using OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Mediator.Cortex.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
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
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
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
                    "https://osito-polar-v1.web.app",
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

// Profiles Bounded Context
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileCommandService, ProfileCommandService>();
builder.Services.AddScoped<IProfileQueryService, ProfileQueryService>();

// IAM Bounded Context
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

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

// Mediator Configuration
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddCortexMediator(
    configuration: builder.Configuration,
    handlerAssemblyMarkerTypes: new[] { typeof(Program) },
    configure: options =>
    {
        options.AddOpenCommandPipelineBehavior(typeof(LoggingCommandBehavior<>));
    });

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString, mysqlOptions => 
            mysqlOptions.MigrationsAssembly("OsitoPolarPlatform.API"))
           .EnableSensitiveDataLogging(isProduction ? false : true)
           .EnableDetailedErrors(isProduction ? false : true);
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Checking for pending migrations...");
        var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        logger.LogInformation("Applied migrations: {Applied}", string.Join(", ", appliedMigrations));
        logger.LogInformation("Pending migrations: {Pending}", string.Join(", ", pendingMigrations));
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying migrations...");
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations: {Message}", ex.Message);
        throw;
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OsitoPolar API V1");
    c.RoutePrefix = string.Empty;
});

if (!isProduction)
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseRequestAuthorization();
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
    Console.WriteLine($"📍 Service Request API: http://0.0.0.0:{port}/api/v1/service-requests");
    Console.WriteLine($"📍 Technician API: http://0.0.0.0:{port}/api/v1/technicians");
}
else
{
    Console.WriteLine($"📍 Swagger: http://localhost:5000/swagger");
    Console.WriteLine($"📍 Equipment API: http://localhost:5000/api/v1/equipments");
}

app.Run();