using API.DTOs;
using Application.Features.Customers.Commands;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Auth
{
    public class RegisterEndpoint : Endpoint<ReqRegisterDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/register");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("auth_strict"));
        }

        public override async Task HandleAsync(ReqRegisterDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new AddUserCommand(req.Fullname, req.Email, req.Password), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(201);
            }
            else
            {
                await Send.ResponseAsync(result.Error, result.StatusCode, ct);
            }
        }
    }
}
