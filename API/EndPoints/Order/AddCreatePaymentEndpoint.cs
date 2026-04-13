using API.DTOs;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Order
{
    public class AddCreatePaymentEndpoint : Endpoint<ReqOrderInfo>
    {
        public IMediator _mediator;
        public AddCreatePaymentEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }
        public override void Configure()
        {
            Post("/order/create-payment");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Options(x => x.RequireRateLimiting("create_payment"));
        }
        public override async Task HandleAsync(ReqOrderInfo req, CancellationToken ct)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _mediator.Send(new CreatePaymentCommand(req.OrderId, (decimal)req.Amount, ipAddress,req.TypePayment,req.Address,req.PhoneNumber), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data, 200, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, statusCode: result.StatusCode, ct);
            }
        }
    }
}
