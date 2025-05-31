using System.Text;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyChat.Configuration;
using MyChat.Data;
using MyChat.Services;

namespace MyChat.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        // Add AWS settings
        services.Configure<AwsSettings>(configuration.GetSection("AWS"));

        // Add AWS S3 client
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var awsSettings = configuration.GetSection("AWS").Get<AwsSettings>();
            var config = new Amazon.S3.AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings?.S3?.Region ?? "us-east-1")
            };
            
            // Check for environment variables first (Docker Compose)
            var envAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var envSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            
            if (!string.IsNullOrEmpty(envAccessKey) && !string.IsNullOrEmpty(envSecretKey))
            {
                // Docker Compose or environment variables
                var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    return new Amazon.S3.AmazonS3Client(envAccessKey, envSecretKey, sessionToken, config);
                }
                else
                {
                    return new Amazon.S3.AmazonS3Client(envAccessKey, envSecretKey, config);
                }
            }
            // Fallback to config file credentials
            else if (!string.IsNullOrEmpty(awsSettings?.S3?.AccessKey) && !string.IsNullOrEmpty(awsSettings?.S3?.SecretKey))
            {
                // Local development with explicit credentials in config
                return new Amazon.S3.AmazonS3Client(awsSettings.S3.AccessKey, awsSettings.S3.SecretKey, config);
            }
            else
            {
                // Production/ECS - use IAM roles or default credential chain
                return new Amazon.S3.AmazonS3Client(config);
            }
        });
        
        // Add services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ISignalRService, SignalRService>();
        services.AddScoped<IImageStorageService, S3ImageStorageService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        
        if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT settings are not configured properly");
        }

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
            
            // Configure JWT for SignalR
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    // If the request is for our SignalR hub...
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

        services.AddCors(options =>
        {
            options.AddPolicy("ChatAppPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
