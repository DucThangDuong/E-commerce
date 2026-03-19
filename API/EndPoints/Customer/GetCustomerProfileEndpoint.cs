using API.Extendsion;
using Application.Features.Customers.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class GetCustomerProfileEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/customer/profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new GetCustomerProfileQuery(userId), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data);
                return;
            }
            await Send.ResponseAsync(null, result.StatusCode);
        }
    }
}
