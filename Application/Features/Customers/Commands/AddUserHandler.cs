using Application.Common;
using Application.Interfaces;
using MassTransit;
using Application.DTOs.Services;
using MediatR;

namespace Application.Features.Customers.Commands
{
    public record AddUserCommand(string Name, string Email, string Password) : IRequest<Result>;

    public class AddUserHandler : IRequestHandler<AddUserCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ICustomerRepository _customerRepository;
        public AddUserHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddUserCommand command, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password) || string.IsNullOrEmpty(command.Name))
            {
                return Result.Failure("Email, Password và Name không được để trống", 400);
            }
            try
            {
                bool isEmailExists = await _customerRepository.EmailExistsAsync(command.Email);
                if (isEmailExists)
                {
                    return Result.Failure("Email đã tồn tại", 400);
                }
                await _customerRepository.AddAsync(command.Email, command.Password, command.Name);

                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                return Result.Failure("Đã xảy ra lỗi nội bộ trong quá trình đăng ký.", 500);
            }
        }
    }
}
