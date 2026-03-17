using API.DTOs;
using Application.Features.Customers.Commands;
using Application.IServices;
using FastEndpoints;

namespace API.Endpoints.Auth;

public class GoogleLoginEndpoint : Endpoint<ReqGoogleLoginDTO>
{
    public AddLoginGoogleCustomerHandler _handler { get; set; } = null!;
    public override void Configure()
    {
        Post("/google");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqGoogleLoginDTO req, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(new AddLoginGoogleCustomerCommand(req.IdToken));
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
