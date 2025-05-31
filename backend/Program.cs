using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Extensions;
using MyChat.Hubs;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure enums to be serialized as strings instead of numbers
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Optional: Use camelCase for property names
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddOpenApi();

// Add health checks
builder.Services.AddHealthChecks();
builder.Services
    .AddDbContext<ChatDbContext>();

// Add SignalR
builder.Services.AddSignalR();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add CORS
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Apply migrations with retry logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ChatDbContext>();
    
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database (attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
            await context.Database.CanConnectAsync();
            logger.LogInformation("Successfully connected to database");
            
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database connection attempt {Attempt}/{MaxRetries} failed", i + 1, maxRetries);
            
            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts", maxRetries);
                throw;
            }
            
            logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Only use HTTPS redirection in development or when explicitly configured
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("ForceHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

// Use CORS
app.UseCors("ChatAppPolicy");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

app.Run();
