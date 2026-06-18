using API.Extensions;
using Application.Features.Carts.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class ReqUpdateCartDto
    {
        public int cartId { get; set; }
        public int quantity { get; set; }
    }

    public class UpdateCartEndpoint : Endpoint<ReqUpdateCartDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/cart/update");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Options(x => x.RequireRateLimiting("cart_strict"));
            AllowFormData();
        }

        public override async Task HandleAsync(ReqUpdateCartDto req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            if (userId == 0 || req.cartId == 0)
            {
                await Send.ResponseAsync(new { message = "Invalid request parameters" }, statusCode: 400, ct);
                return;
            }
            
            var result = await Mediator.Send(new UpdateItemCartCustomerCommand(userId, req.cartId, req.quantity), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
