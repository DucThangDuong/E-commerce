using API.DTOs;
using Application.Features.Order.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.Extensions;
using Application.Common;
using Application.DTOs.Response;

namespace API.EndPoints.Order
{
    public class AddCreatePaymentEndpoint : Endpoint<ReqCreatePayment>
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
        public override async Task HandleAsync(ReqCreatePayment req, CancellationToken ct)
        {
            if (!HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) && req.TypePayment == 1)
            {
                Result Fail = Result.Failure("Missing Header: Idempotency-Key is required.", 400);
                await this.SendApiResponseAsync(Fail, ct, defaultErrorCode: "ERR_MISSING_IDEMPOTENCY_KEY");
                return;
            }

            int userId = HttpContext.User.GetUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var items = req.Items.Select(i => new CartItemRequest(i.ColorId, i.Quantity)).ToList();

            var result = await _mediator.Send(new CreatePaymentCommand(
                items, req.CouponCode, ipAddress, req.TypePayment,
                req.Address, req.PhoneNumber, req.FullName, idempotencyKey.ToString(), userId
            ), ct);

            await this.SendApiResponseAsync(result, ct,
                Message: "Tạo yêu cầu thanh toán thành công",
                ErrorCode: "ERR_PAYMENT_FAILED");
        }
    }
}
