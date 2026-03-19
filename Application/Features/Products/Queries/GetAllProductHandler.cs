using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries
{
    public record GetAllProductQuery(int Skip, int Take) : IRequest<Result<List<ResProductDto>>>;

    public class GetAllProductHandler : IRequestHandler<GetAllProductQuery, Result<List<ResProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ResProductDto>>> Handle(GetAllProductQuery query, CancellationToken ct)
        {
            try
            {
                var products = await _unitOfWork.ProductRepository.GetAllProductsAsync(query.Skip, query.Take, ct);
                return Result<List<ResProductDto>>.Success(products);
            }
            catch (Exception ex)
            {
                return Result<List<ResProductDto>>.Failure($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}", 500);
            }
        }
    }
}
