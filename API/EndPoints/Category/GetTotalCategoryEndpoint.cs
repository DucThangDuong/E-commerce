using API.Extensions;
using Application.DTOs.Response;
using Application.Features.Categories.Queries;
using FastEndpoints;
using MediatR;

namespace API.EndPoints.Category
{
    public class ReqGetTotalCategoryDto
    {
        public int take { get; set; } = 10;
    }

    public class GetTotalCategoryEndpoint : Endpoint<ReqGetTotalCategoryDto, List<ResCategoryDto>>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Get("/category");
            AllowAnonymous();
        }

        public override async Task HandleAsync(ReqGetTotalCategoryDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new GetAllCategoryQuery(req.take), ct);
            await this.SendApiResponseAsync(result, ct);
        }
    }
}