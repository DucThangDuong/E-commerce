using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Brands.Commands
{
    public record AddNewBrandCommand(string Name, string Description, string? LogoUrl) : IRequest<Result>;

    public class AddNewBrandHandler : IRequestHandler<AddNewBrandCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddNewBrandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddNewBrandCommand command, CancellationToken ct)
        {
            var value = await _unitOfWork.BrandRepository.AddNewBrandAsync(command.Name, command.Description, command.LogoUrl);
            if (value != null)
            {
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success();
            }
            else
            {
                return Result.Failure("Failed to add new brand", 500);
            }
        }
    }
}
