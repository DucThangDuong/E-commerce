using API.DTOs;
using Application.Features.Customers.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Auth;

public class LoginEndpoint : Endpoint<ReqLoginDTo>
{
    public IMediator Mediator { get; set; } = null!;

    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqLoginDTo req, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLoginUserQueries(req.Email, req.Password), ct);
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
            await Send.ResponseAsync(result.Data?.AccessToken, result.StatusCode, ct);
        }
        else
        {
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        }
    }
}
