using API.Extensions;
using Application.Features.Order.Commands;
using Application.IServices;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Order
{
    public class PaymentIpnEndpoint : EndpointWithoutRequest
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMediator _mediator;
        public PaymentIpnEndpoint( IVnPayService vnPayService, IMediator mediator) { 
            _vnPayService = vnPayService;
            _mediator = mediator;
        }
        public override void Configure()
        {
            Get("/order/payment-ipn");
            AllowAnonymous();
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            var queryDictionary = HttpContext.Request.Query;
            bool isValidSignature = _vnPayService.ValidateSignature(queryDictionary);
            if (!isValidSignature)
            {
                await Send.ResponseAsync(new { RspCode = "97", Message = "Invalid signature" }, 200, ct);
                return;
            }
            string orderIdStr = queryDictionary["vnp_TxnRef"].ToString();
            string vnpAmountStr = queryDictionary["vnp_Amount"].ToString();
            string responseCode = queryDictionary["vnp_ResponseCode"].ToString();
            string transactionNo = queryDictionary["vnp_TransactionNo"].ToString();
            var command = new ProcessIpnCommand(
                int.Parse(orderIdStr),
                decimal.Parse(vnpAmountStr) / 100,
                responseCode,
                transactionNo
            );
            var result = await _mediator.Send(command, ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
