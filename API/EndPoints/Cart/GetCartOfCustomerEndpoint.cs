using API.Extensions;
using Application.Features.Carts.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class GetCartOfCustomerEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("cart");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Options(x => x.RequireRateLimiting("cart_strict"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new GetItemCartCustomerQuery(userId), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
