using API.DTOs;
using Application.DTOs.Response;
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
            var response = new ApiSuccessResponse<string>
            {
                Data = result.Data?.AccessToken ?? "",
                Message = "Login successful",
            };
            await Send.ResponseAsync(response, result.StatusCode, ct);
        }
        else
        {
            var response = new ApiErrorResponse
            {
                Message = "Login failed",
                ErrorCode = "lOGIN_FAIL",
                Errors = result.Errors
            };
            await Send.ResponseAsync(response, result.StatusCode, ct);
        }
    }
}
