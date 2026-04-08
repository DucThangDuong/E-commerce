using API.DTOs;
using API.Extendsion;
using Application.Features.Carts.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class AddNewCustomerCartEndpoint : Endpoint<ReqCreateCartDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/cart");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqCreateCartDto req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            if (userId == 0 || req.product_id == 0 || req.quantity == 0)
            {
                await Send.ResponseAsync(new { message = "customer_id, product_id and quantity must be provided and greater than 0" }, statusCode: 400, ct);
                return;
            }
            var result = await Mediator.Send(new AddItemCartCustomerCommand(userId, req.product_id, req.quantity), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(null, result.StatusCode);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, statusCode: result.StatusCode, ct);
            }
        }
    }
}
