using API.Extensions;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Purchase
{
    public class CompleteOrderReqDto
    {
        public int OrderId { get; set; }
    }

    public class CompleteOrderEndpoint : Endpoint<CompleteOrderReqDto>
    {
        private readonly IMediator _mediator;

        public CompleteOrderEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/order/complete");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Roles("Admin");
        }

        public override async Task HandleAsync(CompleteOrderReqDto req, CancellationToken ct)
        {
            var result = await _mediator.Send(new CompleteOrderCommand(req.OrderId), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
