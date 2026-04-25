using API.Extensions;
using Application.Features.Products.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Product
{
    public class ReqGetFilteredProductsDto
    {
        public List<int>? CategoryIds { get; set; }
        public List<int>? BrandIds { get; set; }
        public int take { get; set; } = 10;
        public int skip { get; set; } = 0;
    }

    public class GetFilteredProductsEndpoint : Endpoint<ReqGetFilteredProductsDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/product/filter");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("search_strict"));
        }

        public override async Task HandleAsync(ReqGetFilteredProductsDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetFilteredProductsQuery(req.CategoryIds, req.BrandIds, req.skip, req.take), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
