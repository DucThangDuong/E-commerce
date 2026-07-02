using Application.IServices;
using FastEndpoints;

namespace API.EndPoints.Order
{
    public class PaymentCallbackEndpoint : EndpointWithoutRequest
    {
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public PaymentCallbackEndpoint(IVnPayService vnPayService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _configuration = configuration;
        }

        public override void Configure()
        {
            Get("/order/payment-callback");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = _vnPayService.PaymentCallback(HttpContext.Request.Query);

            string frontendBaseUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            string frontendUrl = $"{frontendBaseUrl.TrimEnd('/')}/purchase";
            string redirectUrl;

            if (response.Success)
            {
                redirectUrl = $"{frontendUrl}?tab=completed&payment=success&orderId={response.OrderId}";
            }
            else if (response.ResponseCode == "24")
            {
                redirectUrl = $"{frontendUrl}?tab=cancelled&payment=cancelled&orderId={response.OrderId}";
            }
            else
            {
                redirectUrl = $"{frontendUrl}?tab=pending&payment=failed&orderId={response.OrderId}";
            }

            HttpContext.Response.Redirect(redirectUrl);
            await Task.CompletedTask;
        }
    }
}
