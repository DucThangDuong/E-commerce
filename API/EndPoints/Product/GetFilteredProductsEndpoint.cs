using API.Extensions;
using Application.Features.Products.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Product
{
    public class ReqGetFilteredProductsDto
    {
        [QueryParam]
        public List<int>? CategoryIds { get; set; }
        
        [QueryParam]
        public List<int>? BrandIds { get; set; }
        
        [QueryParam]
        public string? Keyword { get; set; }

        [QueryParam]
        public decimal? MinPrice { get; set; }

        [QueryParam]
        public decimal? MaxPrice { get; set; }

        [QueryParam]
        public int take { get; set; } = 10;
        
        [QueryParam]
        public int skip { get; set; } = 0;
    }

    public class GetFilteredProductsEndpoint : Endpoint<ReqGetFilteredProductsDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/product/filter");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("search_strict"));
        }

        public override async Task HandleAsync(ReqGetFilteredProductsDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetFilteredProductsQuery(req.CategoryIds, req.BrandIds, req.Keyword, req.MinPrice, req.MaxPrice, req.skip, req.take), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
