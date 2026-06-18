using API.Extensions;
using Application.Features.Purchase.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace API.EndPoints.Purchase
{
    public class ReqGetPurchaseDto
    {
        [QueryParam]
        public string? Status { get; set; }
    }

    public class GetPurchasesEndpoint : Endpoint<ReqGetPurchaseDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/purchase");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqGetPurchaseDto req, CancellationToken ct)
        {
            int customerId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new GetPurchasesQuery(customerId, req.Status), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
