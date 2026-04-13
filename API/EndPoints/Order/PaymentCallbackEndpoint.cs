using Application.IServices;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Order
{
    public class PaymentCallbackEndpoint : EndpointWithoutRequest
    {
        private readonly IVnPayService _vnPayService;

        public PaymentCallbackEndpoint(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public override void Configure()
        {
            Get("/order/payment-callback");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = _vnPayService.PaymentCallback(HttpContext.Request.Query);

            if (response.Success)
            {
                await Send.ResponseAsync(new { message = response.Message, orderId = response.OrderId, transactionId = response.TransactionId },200, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = response.Message, orderId = response.OrderId }, 400, ct);
            }
        }
    }
}
