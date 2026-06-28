using API.Extensions;
using Application.Features.Products.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Product
{
    public class GetActivePromotionsEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/promotion/active");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("search_strict"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetActivePromotionsQuery(), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
