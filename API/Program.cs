using Amazon.S3;
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
                x.AddConsumer<SendMail>();

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

                    cfg.ConfigureEndpoints(context);
                });
            });


            //──────────── Redis  ────────────
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["RedisCache"];
                options.InstanceName = "MyApp_";
            });

            // ──────────── Rate Limiting ────────────
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("auth_strict", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(30)
                        }));
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
                });

            builder.Services.AddAuthorization();

            // ──────────── Services (DI) ────────────
            builder.Services.AddScoped<IJWTTokenServices, JwtTokenService>();

            // Email
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddScoped<IEmailSender, MailSender>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Repositories (individual registration for DI into UnitOfWork)
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseFastEndpoints();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hub/notifications");
            app.Run();
        }
    }
}
