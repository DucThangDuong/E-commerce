using API.DTOs;
using Application.Features.Brands.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Brand
{
    public class AddNewBrandEndpoint : Endpoint<ReqCreateBrandDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/brand");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ReqCreateBrandDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new AddNewBrandCommand(req.Name, req.Description, req.LogoUrl), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(null, 201, ct);
                return;
            }
            await Send.ResponseAsync(new { message = result.Errors }, result.StatusCode, ct);
        }
    }
}
