using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands
{
    public record AddFeaturedProductCommand(
        int ProductId,
        int? DisplayOrder,
        DateTime? StartDate,
        DateTime? EndDate) : IRequest<Result>;

    public class AddFeaturedProductHandler : IRequestHandler<AddFeaturedProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddFeaturedProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddFeaturedProductCommand command, CancellationToken ct)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.ProductExistsAsync(command.ProductId, ct);
                if (product == false)
                {
                    return Result.Failure("Product not found.", 404);
                }

                FeaturedProduct featuredProduct = new FeaturedProduct
                {
                    ProductId = command.ProductId,
                    DisplayOrder = command.DisplayOrder,
                    StartDate = command.StartDate,
                    EndDate = command.EndDate,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProductRepository.AddFeaturedProductAsync(featuredProduct, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occurred while adding the featured product: {ex.Message}", 500);
            }
        }
    }
}
