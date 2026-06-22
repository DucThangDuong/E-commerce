using API.Extensions;
using Application.Features.Order.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Purchase
{
    public class GetCancellationReasonsEndpoint : EndpointWithoutRequest
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/cancellation-reasons");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetAllCancellationReasonsQuery(), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
