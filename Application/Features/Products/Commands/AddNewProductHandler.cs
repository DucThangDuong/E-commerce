using Application.Common;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands
{
    public record AddNewProductCommand(int category_id ,string name ,string? description ,decimal base_price,int stock_quatity,List<IFormFile>? images );
    public class AddNewProductHandler:ICommandHandler<AddNewProductCommand>
    {
        private readonly IUnitOfWork _context;
        public IStorageService StorageService;
        public AddNewProductHandler(IUnitOfWork context , IStorageService storageService) {
            _context = context;
            StorageService = storageService;
        }

        public async Task<Result> HandleAsync(AddNewProductCommand command, CancellationToken ct = default)
        {
            try
            {

                bool hasCategory = _context.Context.Categories.AsNoTracking().Any(c => c.CategoryId == command.category_id);
                if (!hasCategory)
                {
                    return Result.Failure("Category not found", 404);
                }
                var imageUrls = new List<string>();
                if (command.images != null && command.images.Any())
                {
                    foreach (var image in command.images)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var url = await StorageService.UploadFileAsync(image.OpenReadStream(), fileName, image.ContentType, StorageType.product);
                        imageUrls.Add(url);
                    }
                }
                Product newProduct = new Product
                {
                    CategoryId = command.category_id,
                    Name = command.name,
                    Description = command.description,
                    BasePrice = command.base_price,
                    Inventory = new Inventory
                    {
                        StockQuantity = command.stock_quatity,
                        ReservedQuantity = 0,
                        LastUpdated = DateTime.UtcNow
                    },
                    ProductImages = imageUrls?.Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        IsPrimary = index == 0,
                        DisplayOrder = index,
                        UploadedAt = DateTime.UtcNow
                    }).ToList() ?? new List<ProductImage>()
                };
                await _context.ProductRepository.AddAsync(newProduct);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred while adding the product: {ex.Message}", 500);
            }

        }
    }
}
