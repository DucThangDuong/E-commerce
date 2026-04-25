using Application.Common;
using Application.DTOs.Response;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands
{
    public record AddNewProductCommand(
        int CategoryId, 
        string Name, 
        string? Description, 
        decimal BasePrice, 
        int StockQuantity, 
        int BrandId ,
        List<FileUploadDto>? Images) : IRequest<Result>;

    public class AddNewProductHandler : IRequestHandler<AddNewProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        public AddNewProductHandler(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
        }

        public async Task<Result> Handle(AddNewProductCommand command, CancellationToken ct)
        {
            try
            {
                bool hasCategory = await _unitOfWork.CategoryRepository.CategoryExistsAsync(command.CategoryId, ct);
                if (!hasCategory)
                {
                    return Result.Failure("Category not found", 404);
                }

                var imageUrls = new List<string>();
                if (command.Images != null && command.Images.Any())
                {
                    foreach (var image in command.Images)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var url = await _storageService.UploadFileAsync(image.Stream, fileName, image.ContentType, StorageType.product);
                        imageUrls.Add(url);
                    }
                }

                Product newProduct = new Product
                {
                    CategoryId = command.CategoryId,
                    Name = command.Name,
                    Description = command.Description,
                    BasePrice = command.BasePrice,
                    BrandId = command.BrandId,
                    Inventory = new Inventory
                    {
                        StockQuantity = command.StockQuantity,
                        ReservedQuantity = 0,
                        LastUpdated = DateTime.UtcNow
                    },
                    ProductImages = imageUrls.Select((url, index) => new ProductImage
                    {
                        ImageUrl = url,
                        IsPrimary = index == 0,
                        DisplayOrder = index,
                        UploadedAt = DateTime.UtcNow
                    }).ToList()
                };

                await _unitOfWork.ProductRepository.AddAsync(newProduct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred while adding the product: {ex.Message}", 500);
            }
        }
    }
}
