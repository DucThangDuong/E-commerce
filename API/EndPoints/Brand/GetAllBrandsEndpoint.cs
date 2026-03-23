using Application.Features.Brands.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Brand
{
    public class ReqGetBrandDto
    {
        public int take { get; set; } = 10;
        public int skip { get; set; } = 0;
    }

    public class GetAllBrandsEndpoint : Endpoint<ReqGetBrandDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/brand");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetBrandDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetAllBrandsQuery(req.take), ct);
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
