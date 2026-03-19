using API.Extendsion;
using Application.Features.Carts.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class ReqDeleteCartDto { public int productId { get; set; } }

    public class DeleteCartOfUserEndpoint : Endpoint<ReqDeleteCartDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Delete("/cart");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqDeleteCartDto req, CancellationToken ct)
        {
            var userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new DeleteItemCartCustomerCommand(userId, req.productId), ct);
            if (result.IsSuccess)
            {
                await Send.NoContentAsync();
                return;
            }
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        }
    }
}
