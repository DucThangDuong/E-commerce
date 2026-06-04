using API.DTOs;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Extensions;
using Application.DTOs.Response;

namespace API.EndPoints.Order
{
    public class CalculateOrderEndpoint : Endpoint<ReqCalculateOrder>
    {
        public IMediator Mediator { get; set; }
        public CalculateOrderEndpoint(IMediator mediator)
        {
            Mediator = mediator;
        }
        public override void Configure()
        {
            Post("/order/calculate");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }
        public override async Task HandleAsync(ReqCalculateOrder req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var items = req.Items.Select(i => new CartItemRequest(i.ColorId, i.Quantity)).ToList();
            var result = await Mediator.Send(new CalculateOrderCommand(items, req.CouponCode, userId), ct);

            await this.SendApiResponseAsync(result, ct);
        }
    }
}
