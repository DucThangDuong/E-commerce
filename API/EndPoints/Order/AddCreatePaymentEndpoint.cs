using API.DTOs;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using API.Extensions;
using Application.Common;

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
            if (!HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
            {
                AddError("Missing Header: Idempotency-Key is required.");
                Result Fail = Result.Failure("Missing Header: Idempotency-Key is required.", 400);
                await this.SendApiResponseAsync(Fail, ct, defaultErrorCode: "ERR_MISSING_IDEMPOTENCY_KEY");
                return;
            }
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            Result<string> result = await _mediator.Send(new CreatePaymentCommand(req.ReservationId, (decimal)req.Amount, ipAddress, req.TypePayment,
                req.Address, req.PhoneNumber, req.FullName, idempotencyKey.ToString()), ct);
            await this.SendApiResponseAsync<string>(result, ct,
                Message: "Tạo yêu cầu thanh toán thành công",
                ErrorCode: "ERR_PAYMENT_FAILED");
        }
    }
}
