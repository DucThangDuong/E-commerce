using API.Extensions;
using Application.Features.Products.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Product
{
    public class ReqAddFeaturedProductDto
    {
        public int ProductId { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AddFeaturedProductEndpoint : Endpoint<ReqAddFeaturedProductDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/product/featured");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Roles("Admin");
        }

        public override async Task HandleAsync(ReqAddFeaturedProductDto req, CancellationToken ct)
        {
            var result = await Mediator.Send(new AddFeaturedProductCommand(
                req.ProductId, req.DisplayOrder, req.StartDate, req.EndDate), ct);
                
            await this.SendApiResponseAsync(result, ct);
        }
    }
}
