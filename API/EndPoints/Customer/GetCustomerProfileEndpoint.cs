using API.DTOs;
using API.Extendsion;
using Application.Features.Categories.Command;
using Application.Features.Customers.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace API.EndPoints.Customer
{

    public class GetCustomerProfileEndpoint : EndpointWithoutRequest
    {
        public GetCustomerProfileHandler _handler { get; set; } = null!;
        public override void Configure()
        {
            Get("/customer/profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await _handler.HandleAsync(new GetCustomerProfileQuery(userId));
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data);
                return;
            }
            await Send.ResponseAsync(null,result.StatusCode);
        }
    }
}
