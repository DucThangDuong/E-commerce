using API.DTOs;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Extensions;
using Application.Common;

namespace API.EndPoints.Order
{
    public class AddNewOrderEndpoint : Endpoint<ReqAddNewOrder>
    {
        public IMediator Mediator { get; set; }
        public AddNewOrderEndpoint(IMediator mediator)
        {
            Mediator = mediator;
        }
        public override void Configure()
        {
            Post("/order");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Options(x => x.RequireRateLimiting("order_strict"));
        }
        public override async Task HandleAsync(ReqAddNewOrder req, CancellationToken ct)
        {
            Dictionary<int, int> items = new Dictionary<int, int>();
            int userId = HttpContext.User.GetUserId();
            foreach (var item in req.Items)
            {
                if (items.ContainsKey(item.ProductId))
                {
                    items[item.ProductId] += item.Quantity;
                }
                else
                {
                    items.Add(item.ProductId, item.Quantity);
                }
            }
            var result = await Mediator.Send(new AddOrderItemCustomerCommand(userId, items), ct);

            await this.SendApiResponseAsync(result, ct);
        }
    }
}
