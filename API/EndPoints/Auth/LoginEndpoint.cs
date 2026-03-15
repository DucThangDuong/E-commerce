using API.DTOs;
using Application.Features.Customer.Queries;
using FastEndpoints;

namespace API.EndPoints.Auth;

public class LoginEndpoint : Endpoint<ReqLoginDTo>
{
    public GetLoginUserHandler Handler {get;set;}=null!;
    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
        Options(x=>x.RequireRateLimiting("auth_strict"));
    }
    public override async Task HandleAsync(ReqLoginDTo req, CancellationToken ct)
    {
        var result= await Handler.HandleAsync(new LoginCommand(req.Email,req.Password), ct);
        if(result.IsSuccess)
        {
            await Send.ResponseAsync(result.StatusCode);
        }
        else
        {
            await Send.ResponseAsync(result.Error, result.StatusCode, ct);
        }
    }
}
