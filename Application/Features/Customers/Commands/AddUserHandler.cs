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
        private readonly IPublishEndpoint _publishEndpoint;

        public AddUserHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result> Handle(AddUserCommand command, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password) || string.IsNullOrEmpty(command.Name))
            {
                return Result.Failure("Email, Password và Name không được để trống", 400);
            }
            try
            {
                bool isEmailExists = await _unitOfWork.CustomerRepository.EmailExistsAsync(command.Email);
                if (isEmailExists)
                {
                    return Result.Failure("Email đã tồn tại", 400);
                }
                await _unitOfWork.CustomerRepository.AddAsync(command.Email, command.Password, command.Name);
                var orderEvent = new SendMail(command.Email, "Chào mừng bạn đến với Food Delivery App!",
                    $"Xin chào {command.Name}!\nCảm ơn bạn đã đăng ký tài khoản tại Food Delivery App. Chúng tôi rất vui được phục vụ bạn!");
                await _publishEndpoint.Publish(orderEvent, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success(201);
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }
}
