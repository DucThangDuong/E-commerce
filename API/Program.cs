using API.DTOs;
using API.Extensions;
using API.Logging;
using API.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Services;
using Serilog;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Destructure.With<SensitiveDataDestructuringPolicy>());

            // ──────────── Dependency Injection Bootstrapping ────────────
            builder.Services.AddApiConfiguration(builder.Configuration)
                            .AddSwaggerConfiguration()
                            .AddDatabaseConfiguration(builder.Configuration, builder.Environment)
                            .AddApplicationServices(builder.Configuration)
                            .AddMessageBrokerConfiguration(builder.Configuration)
                            .AddCacheConfiguration(builder.Configuration)
                            .AddRateLimitingConfiguration()
                            .AddJwtAuthentication(builder.Configuration);

            var app = builder.Build();

            // ──────────── Global Exception Handling ────────────
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error is Exception ex)
                    {
                        Log.Error(ex, "Unhandled exception occurred while processing request");
                    }

                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    var response = new ApiErrorResponse
                    {
                        Message = "Hệ thống đang gặp sự cố, vui lòng thử lại sau.",
                        ErrorCode = "ERR_INTERNAL_SERVER",
                        TraceId = context.TraceIdentifier
                    };
                    await context.Response.WriteAsJsonAsync(response);
                });
            });

            // ──────────── Middlewares ────────────
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseMiddleware<XssSanitizationMiddleware>();
            app.UseMiddleware<LogContextMiddleware>();
            
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCors("CORS");
            app.UseHttpsRedirection();
            
            app.UseAuthentication();
            app.UseMiddleware<AccessTokenBlacklistMiddleware>();
            app.UseAuthorization();
            
            app.UseRateLimiter();
            
            app.UseFastEndpoints(c => 
            {
                c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
                {
                    return new ApiErrorResponse
                    {
                        Message = "Dữ liệu đầu vào không hợp lệ.",
                        ErrorCode = "ERR_VALIDATION_FAILED",
                        Errors = failures.Select(f => new 
                        {
                            field = f.PropertyName,
                            message = f.ErrorMessage
                        }),
                        TraceId = ctx.TraceIdentifier
                    };
                };
            });
            
            app.UseSwaggerGen();
            app.MapControllers();
            app.MapHub<NotificationHub>("/notifications");
            app.Run();
        }
    }
}
