using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands
{
    public record AddNewCategoryCommand(string Name, string Description, string? Picture) : IRequest<Result>;

    public class AddNewCategoryHandler : IRequestHandler<AddNewCategoryCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddNewCategoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddNewCategoryCommand command, CancellationToken ct)
        {
            var value = await _unitOfWork.CategoryRepository.AddNewCategoryAsync(command.Name, command.Description, command.Picture);
            if (value != null)
            {
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            else
            {
                return Result.Failure("Failed to add new category", 500);
            }
        }
    }
}
