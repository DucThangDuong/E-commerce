using API.DTOs;
using API.Extendsion;
using Application.Features.Customers.Commands;
using Domain.Entities;
using FastEndpoints;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class UpdateCustomerProfileEndpoint : Endpoint<ReqUpdateCustomerProfile>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Put("/customer/profile");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqUpdateCustomerProfile req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new UpdateCustomerProfileCommand(userId, req.Name, req.PhoneNumber, req.Address), ct);

            if (result.IsSuccess)
            {
                await Send.ResponseAsync(new { message = "Cập nhật thông tin thành công" }, statusCode: 200, cancellation: ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, statusCode: result.StatusCode, cancellation: ct);
            }
        }
    }
}
