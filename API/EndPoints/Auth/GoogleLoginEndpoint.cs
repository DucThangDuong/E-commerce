using API.DTOs;
using Application.Features.Customers.Commands;
using FastEndpoints;
using MediatR;

namespace API.Endpoints.Auth;

public class GoogleLoginEndpoint : Endpoint<ReqGoogleLoginDTO>
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Post("/google");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqGoogleLoginDTO req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AddLoginGoogleCustomerCommand(req.IdToken), ct);
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
            await Send.ResponseAsync(result.Data!, result.StatusCode, ct);
        }
        else
        {
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        }
    }
}
