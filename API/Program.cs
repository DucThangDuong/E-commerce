using Application.Consumers;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
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
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddFastEndpoints();
            builder.Services.AddDbContext < EcommerceOrderSystemContext > (options =>
            {
                options.UseSqlServer(builder.Configuration["ConnectionStrings:Ecommerce"]);
            });
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
                    //    options.Events = new JwtBearerEvents
                    //    {
                    //        OnMessageReceived = context =>
                    //        {
                    //            var accessToken = context.Request.Query["access_token"];
                    //            var path = context.HttpContext.Request.Path;
                    //            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                    //            {
                    //                context.Token = accessToken;
                    //            }
                    //            return Task.CompletedTask;
                    //        }
                    //    };
                });
            builder.Services.AddScoped < IJWTTokenServices, JwtTokenService > ();

            // Đăng ký cấu hình Email
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddScoped<IEmailSender, MailSender>();

            builder.Services.AddScoped < IUnitOfWork, UnitOfWork > ();
            builder.Services.AddScoped <ICustomerRepository,CustomerRepository> ();
            builder.Services.AddScoped <Application.Features.Customer.Commands.AddUserHandler> ();
            builder.Services.AddScoped < Application.Features.Customer.Queries.GetLoginUserHandler > ();
            builder.Services.AddHttpContextAccessor();
            var app = builder.Build();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseFastEndpoints();
            app.MapControllers();

            app.Run();
        }
    }
}

