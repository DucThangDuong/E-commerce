using API.DTOs;
using Application.Features.Customer.Commands;
using Application.Features.Customer.Queries;
using FastEndpoints;

namespace API.EndPoints.Auth
{
    public class RegisterEndpoint : Endpoint<ReqRegisterDto>
    {
        public AddUserHandler Handler { get; set; } = null!;
        public override void Configure()
        {
            Post("/register");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("auth_strict"));
        }
        public override async Task HandleAsync(ReqRegisterDto req, CancellationToken ct)
        {
            var result = await Handler.HandleAsync(new AddUserCommand(req.Fullname,req.Email, req.Password), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.StatusCode);
            }
            else
            {
                await Send.ResponseAsync(result.Error, result.StatusCode, ct);
            }
        }
    }
}
