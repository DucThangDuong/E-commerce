using API.Extendsion;
using Application.Features.Carts.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Cart
{
    public class GetCartOfCustomerEndpoint:EndpointWithoutRequest
    {
        public GetItemCartCustomerHandler _handler { get; set; } = null!;
        public override void Configure()
        {
            Get("cart");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            int userId = HttpContext.User.GetUserId();
            var result = await _handler.HandleAsync(new GetItemCartCustomerQuery(userId));
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data,200);
                return;
            }
            await Send.ResponseAsync(null, result.StatusCode);
        }
    }
}
