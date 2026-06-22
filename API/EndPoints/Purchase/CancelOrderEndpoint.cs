using API.Extensions;
using Application.DTOs.Request;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Purchase
{
    public class CancelOrderEndpoint : Endpoint<CancelOrderReqDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/order/cancel");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancelOrderReqDto req, CancellationToken ct)
        {
            int customerId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new CancelOrderCommand(req.OrderId, req.Reason, customerId.ToString()), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
