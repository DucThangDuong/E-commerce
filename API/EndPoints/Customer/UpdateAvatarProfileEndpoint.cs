using API.DTOs;
using API.Extensions;
using Application.Common;
using Application.Features.Customers.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Customer
{
    public class UpdateAvatarProfileEndpoint : Endpoint<ResUpdateAvatarProfile>
    {
        public IMediator _mediator;
        public UpdateAvatarProfileEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }
        public override void Configure()
        {
            Put("/customer/profile/avatar");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            AllowFileUploads();
        }
        public override async Task HandleAsync(ResUpdateAvatarProfile req, CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            Result<string> result = await _mediator.Send(new UpdateAvatarProfileCommand(req.AvatarFile!, userId), ct);
            await this.SendApiResponseAsync<string>(result, ct);
        }
    }
}
