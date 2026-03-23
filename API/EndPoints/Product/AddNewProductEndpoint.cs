using API.DTOs;
using Application.DTOs.Response;
using Application.Features.Products.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.EndPoints.Product
{
    public class AddNewProductEndpoint : Endpoint<ReqCreateProductDto>
    {
        public IMediator Mediator { get; set; } = null!;

        public override void Configure()
        {
            Post("/product");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Roles("Admin");
            AllowFileUploads();
        }

        public override async Task HandleAsync(ReqCreateProductDto req, CancellationToken ct)
        {
            List<FileUploadDto>? fileUploads = null;
            if (req.images != null && req.images.Any())
            {
                fileUploads = req.images.Select(f => new FileUploadDto
                {
                    Stream = f.OpenReadStream(),
                    FileName = f.FileName,
                    ContentType = f.ContentType
                }).ToList();
            }

            var result = await Mediator.Send(new AddNewProductCommand(
                req.category_id, req.name, req.description, req.base_price, req.stock_quantity,req.brand_id, fileUploads), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(null, 201, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            }
        }
    }
}
