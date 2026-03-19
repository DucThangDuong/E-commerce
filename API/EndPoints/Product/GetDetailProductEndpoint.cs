using Application.Features.Products.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Product
{
    public class ReqGetDetalProductDto
    {
        public int productId { get; set; }
    }

    public class GetDetailProductEndpoint : Endpoint<ReqGetDetalProductDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/product/detail");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetDetalProductDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetDetailProductQuery(req.productId), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data, 200, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            }
        }
    }
}
