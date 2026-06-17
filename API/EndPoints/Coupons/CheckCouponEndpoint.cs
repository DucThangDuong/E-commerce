using API.DTOs;
using API.Extensions;
using Application.Common;
using Application.DTOs.Response;
using Application.Features.Coupons.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.EndPoints.Coupons
{
    public class CheckCouponEndpoint : Endpoint<ReqCheckCoupon>
    {
        public IMediator Mediator { get; set; }
        public CheckCouponEndpoint(IMediator mediator)
        {
            Mediator = mediator;
        }

        public override void Configure()
        {
            Post("/coupons/check");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqCheckCoupon req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var items = req.Items.Select(i => new CartItemRequest(i.ColorId, i.Quantity)).ToList();

            var result = await Mediator.Send(new CheckCouponCommand(req.CouponCode, items, userId), ct);

            await this.SendApiResponseAsync(result, ct);
        }
    }
}
