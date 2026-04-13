using Amazon.S3;
using API.Middleware;
using Application.Consumers;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Threading.RateLimiting;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ──────────── CORS ────────────
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("CORS", options =>
                {
                    options
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddFastEndpoints();
            builder.Services.AddDbContext<EcommerceOrderSystemContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["ConnectionStrings:Ecommerce"]);
            });

            // ──────────── MediatR (auto-scan all handlers) ────────────
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<Application.Features.Customers.Commands.AddUserHandler>();
            });

            // ──────────── MassTransit + RabbitMQ ────────────
            builder.Services.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();
                x.AddConsumer<SendMail>();
                x.AddConsumer<CheckOrderExpirationConsumer>();

                x.AddEntityFrameworkOutbox<EcommerceOrderSystemContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:HostName"] ?? "", "/", h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:UserName"] ?? "");
                        h.Password(builder.Configuration["RabbitMQ:Password"] ?? "");
                    });

                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            });


            //──────────── Redis  ────────────
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["RedisCache"];
                options.InstanceName = "MyApp_";
            });
            var redisConnectionString = builder.Configuration["RedisCache"] ?? "localhost:6379";
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(redisConnectionString);
            });
            builder.Services.AddSingleton<ICacheService, CacheService>();

            // ──────────── Rate Limiting ────────────
            builder.Services.AddRateLimiter(options =>
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

            // ──────────── Authentication (JWT) ────────────
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var key = Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"] ?? string.Empty);
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

            builder.Services.AddAuthorization();

            // ──────────── Services (DI) ────────────
            builder.Services.AddScoped<IJWTTokenServices, JwtTokenService>();

            // Email
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddScoped<IEmailSender, MailSender>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IVnPayService, VnPayService>();

            // Repositories (individual registration for DI into UnitOfWork)
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IInventoryRepository,InventoryRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAppReadDbContext>(sp => sp.GetRequiredService<EcommerceOrderSystemContext>());

            // Storage (MinIO / S3)
            var storageConfig = builder.Configuration.GetSection("Storage");
            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = storageConfig["ServiceUrl"],
                    ForcePathStyle = true
                };
                return new AmazonS3Client(
                    storageConfig["AccessKey"],
                    storageConfig["SecretKey"], config);
            });
            builder.Services.AddSingleton<IStorageService, S3StorageService>();
            //hub
            builder.Services.AddSignalR();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // ──────────── Global Exception Handling ────────────
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred. Please try again later." });
                });
            });

            app.UseRouting();
            app.UseCors("CORS");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMiddleware<AccessTokenBlacklistMiddleware>();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseFastEndpoints();
            app.MapControllers();
            app.MapHub<NotificationHub>("/notifications");
            app.Run();
        }
    }
}
