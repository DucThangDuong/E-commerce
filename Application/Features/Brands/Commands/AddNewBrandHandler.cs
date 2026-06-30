using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Brands.Commands
{
    public record AddNewBrandCommand(string Name, string Description, string? LogoUrl) : IRequest<Result>;

    public class AddNewBrandHandler : IRequestHandler<AddNewBrandCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IBrandRepository _brandRepository;
        public AddNewBrandHandler(IUnitOfWork unitOfWork, IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddNewBrandCommand command, CancellationToken ct)
        {
            var value = await _brandRepository.AddNewBrandAsync(command.Name, command.Description, command.LogoUrl);
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
