using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries
{
    public record GetDetailProductQuery(int ProductId) : IRequest<Result<ResProductDto>>;

    public class GetDetailProductHandler : IRequestHandler<GetDetailProductQuery, Result<ResProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDetailProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ResProductDto>> Handle(GetDetailProductQuery query, CancellationToken ct)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetProductDetailAsync(query.ProductId, ct);
                if (product == null)
                {
                    return Result<ResProductDto>.Failure("Product not found", 404);
                }
                return Result<ResProductDto>.Success(product);
            }
            catch (Exception ex)
            {
                return Result<ResProductDto>.Failure($"Lỗi khi lấy chi tiết sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
