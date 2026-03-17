using API.DTOs;
using Application.Features.Products.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Application.IServices;
using Application.DTOs.Services;

namespace API.EndPoints.Product
{
    public class AddNewProductEndpoint : Endpoint<ReqCreateProductDto>
    {
        public AddNewProductHandler Handler { get; set; } = null!;
        public override void Configure()
        {
            Post("/product");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            AllowFileUploads();
        }
        public override async Task HandleAsync(ReqCreateProductDto req, CancellationToken ct)
        {
            var result = await Handler.HandleAsync(new AddNewProductCommand(req.category_id, req.name, req.description, req.base_price,req.stock_quantity, req.images));
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
