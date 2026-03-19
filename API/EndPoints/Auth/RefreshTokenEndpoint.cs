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
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        var result = await Mediator.Send(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsSuccess)
        {
            await Send.ResponseAsync(new { success = true, accessToken = result.Data!.AccessToken }, 200, ct);
        }
        else
        {
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        }
    }
}
