using Application.Features.Customers.Queries;
using FastEndpoints;
using MediatR;

namespace API.Endpoints.Auth;

public class RefreshTokenEndpoint : EndpointWithoutRequest
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Post("/refresh-token");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        var result = await Mediator.Send(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsSuccess)
        {
            if (result.Data != null)
            {
                HttpContext.Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = result.Data.RefreshTokenExpiryTime,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    IsEssential = true
                });
            }
            await Send.ResponseAsync(new { success = true, accessToken = result.Data!.AccessToken }, 200, ct);
        }
        else
        {
            await Send.ResponseAsync(new { message = result.Errors }, result.StatusCode, ct);
        }
    }
}
