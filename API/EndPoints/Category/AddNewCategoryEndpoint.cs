using API.DTOs;
using Application.Features.Categories.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Category
{
    public class AddNewCategoryEndpoint : Endpoint<ReqCreateCategoryDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/category");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Roles("Admin");
        }

        public override async Task HandleAsync(ReqCreateCategoryDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new AddNewCategoryCommand(req.Name, req.Description, req.Picture), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(null, 201, ct);
                return;
            }
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        }
    }
}
