using Application.Common;
using Application.Interfaces;
using MassTransit;
using Application.DTOs.Services;
namespace Application.Features.Customer.Commands;
public record AddUserCommand(string Name, string Email, string Password);

public class AddUserHandler : ICommandHandler<AddUserCommand>
{
    private readonly IUnitOfWork _context;
    private readonly IPublishEndpoint _publishEndpoint;
    public AddUserHandler(IUnitOfWork context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task<Result> HandleAsync(AddUserCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password) || string.IsNullOrEmpty(command.Name))
        {
            return Result.Failure("Email, Password và Name không được để trống", 400);
        }
        try
        {
            bool isEmailExists = await _context.CustomerRepository.EmailExistsAsync(command.Email);
            if (isEmailExists)
            {
                return Result.Failure("Email đã tồn tại", 400);
            }
            await _context.CustomerRepository.AddAsync(command.Email, command.Password, command.Name);
            var orderEvent = new SendMail(command.Email, "Chào mừng bạn đến với Food Delivery App!", 
                $"Xin chào {command.Name}!\nCảm ơn bạn đã đăng ký tài khoản tại Food Delivery App. Chúng tôi rất vui được phục vụ bạn!");
            await _publishEndpoint.Publish(orderEvent);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Lỗi server: " + ex.Message, 500);
        }
    }
}

