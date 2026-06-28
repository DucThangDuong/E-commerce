using API.Extensions;
using Application.Features.Customers.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class GetMyVehiclesEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/customer/my-vehicles");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new GetMyVehiclesQuery(userId), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
