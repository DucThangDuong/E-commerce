using API.DTOs;
using API.Extensions;
using Application.Features.Customers.Commands;
using Domain.Entities;
using FastEndpoints;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class UpdateCustomerNameEndpoint : Endpoint<ReqUpdateCustomerName>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Put("/customer/profile/name");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqUpdateCustomerName req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new UpdateCustomerNameCommand(userId, req.Name), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }

    public class UpdateCustomerPhoneEndpoint : Endpoint<ReqUpdateCustomerPhone>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Put("/customer/profile/phone");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqUpdateCustomerPhone req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new UpdateCustomerPhoneCommand(userId, req.PhoneNumber), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }

    public class UpdateCustomerAddressEndpoint : Endpoint<ReqUpdateCustomerAddress>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Put("/customer/profile/address");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqUpdateCustomerAddress req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new UpdateCustomerAddressCommand(userId, req.Address), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }

    public class UpdateCustomerPasswordEndpoint : Endpoint<ReqUpdateCustomerPassword>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Put("/customer/profile/password");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqUpdateCustomerPassword req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await Mediator.Send(new UpdateCustomerPasswordCommand(userId, req.OldPassword, req.NewPassword), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
