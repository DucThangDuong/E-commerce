using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Categories.Queries
{
    public record GetAllCategoryQuery(int Take) : IRequest<Result<List<ResCategoryDto>>>;

    public class GetAllCategoryHandler : IRequestHandler<GetAllCategoryQuery, Result<List<ResCategoryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCategoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ResCategoryDto>>> Handle(GetAllCategoryQuery query, CancellationToken ct)
        {
            try
            {
                var result = await _unitOfWork.CategoryRepository.GetAllCategoriesAsync(query.Take, ct);
                return Result<List<ResCategoryDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ResCategoryDto>>.Failure(ex.Message);
            }
        }
    }
}
