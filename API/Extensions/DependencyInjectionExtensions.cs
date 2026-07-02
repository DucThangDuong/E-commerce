using Application.Behaviors;
using Application.Consumers;
using Application.Features.Customers.Commands;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

namespace API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(option =>
        {
            option.AddPolicy("CORS", options =>
            {
                options
                .WithOrigins("https://e-commerce-frontend-umber-eight.vercel.app",
                       "http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddFastEndpoints();
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "E-commerce API";
                s.Version = "v1";
            };
            o.EnableJWTBearerAuth = true;
        });
        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<EcommerceContext>(options =>
        {
            var connectionString = environment.IsProduction() 
                ? configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") 
                : configuration.GetConnectionString("Ecommerce");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<AddUserHandler>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Services (DI)
        services.AddScoped<IJWTTokenServices, JwtTokenService>();

        // Email
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        services.AddSingleton<IEmailSender, MailSender>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IVnPayService, VnPayService>();

        // Repositories (individual registration for DI into UnitOfWork)
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderShippingDetailRepository, OrderShippingDetailRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppReadDbContext>(sp => sp.GetRequiredService<EcommerceContext>());

        services.AddScoped<IBlobService, AzureBlobService>();

        // SignalR
        services.AddSignalR();
        return services;
    }

    public static IServiceCollection AddMessageBrokerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddDelayedMessageScheduler();
            x.AddConsumer<SendMailConsumer>();
            x.AddConsumer<CheckOrderExpirationConsumer>();

            x.AddEntityFrameworkOutbox<EcommerceContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:HostName"] ?? "", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"] ?? "");
                    h.Password(configuration["RabbitMQ:Password"] ?? "");
                });

                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["RedisCache"];
        });
        var redisConnectionString = configuration["RedisCache"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString, true);
            redisConfig.AbortOnConnectFail = false;
            redisConfig.ConnectRetry = 5;
            return ConnectionMultiplexer.Connect(redisConfig);
        });
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            var getPartitionKey = (HttpContext httpContext) =>
            {
                var isAuth = httpContext.User.Identity?.IsAuthenticated == true;
                string? userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (isAuth && !string.IsNullOrEmpty(userId))
                {
                    return $"user_{userId}";
                }
                
                return $"ip_{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
            };

            // Global Limiter
            options.AddPolicy("auth_strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"auth_{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(30)
                    }));

            options.AddPolicy("order_strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"order_{getPartitionKey(httpContext)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 4,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(60)
                    }));

            options.AddPolicy("create_payment", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"payment_{getPartitionKey(httpContext)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("search_strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"search_{getPartitionKey(httpContext)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 15,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(10)
                    }));

            options.AddPolicy("cart_strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"cart_{getPartitionKey(httpContext)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(10)
                    }));
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = Encoding.UTF8.GetBytes(configuration["SecretKey"] ?? string.Empty);
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                    {
                        context.Token = accessToken; 
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
