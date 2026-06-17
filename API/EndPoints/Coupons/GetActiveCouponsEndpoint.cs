using API.Extensions;
using Application.Features.Coupons.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace API.EndPoints.Coupons
{
    public class GetActiveCouponsEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; }
        public GetActiveCouponsEndpoint(IMediator mediator)
        {
            Mediator = mediator;
        }

        public override void Configure()
        {
            Get("/coupons/active");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetActiveCouponsQuery(), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
