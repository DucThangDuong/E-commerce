using API.Extensions;
using Application.Features.Order.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading;
using System.Threading.Tasks;

namespace API.EndPoints.Order
{
    public class ReqGetOrderDetailDto
    {
        public int OrderId { get; set; }
    }

    public class GetOrderDetailEndpoint : Endpoint<ReqGetOrderDetailDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/order/{orderId}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqGetOrderDetailDto req, CancellationToken ct)
        {
            var userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new GetOrderDetailQuery(userId, req.OrderId), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
