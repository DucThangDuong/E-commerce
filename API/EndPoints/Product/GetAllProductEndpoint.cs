using Application.Features.Products.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Product
{
    public class ReqGetProductDto
    {
        public int take { get; set; } = 10;
        public int skip { get; set; } = 0;
    }

    public class GetAllProductEndpoint : Endpoint<ReqGetProductDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/product");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetProductDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetAllProductQuery(req.skip, req.take), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data!, 200, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            }
        }
    }
}
