using API.DTOs;
using API.Extensions;
using Application.Features.Customers.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Auth;

public class LogoutEndpoint : EndpointWithoutRequest
{
    public IMediator Mediator { get; set; } = null!;
    public override void Configure()
    {
        Post("/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        string? accessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        var result = await Mediator.Send(new UpdateRevokeRefreshTokenCommand(userId, accessToken));
        if (result.IsSuccess)
        {
            HttpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                IsEssential = true
            });
        }
        await this.SendApiResponseAsync(result,ct);
    }
}

