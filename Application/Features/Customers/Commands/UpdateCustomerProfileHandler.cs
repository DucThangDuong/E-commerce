using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Customers.Commands
{
    public record UpdateCustomerNameCommand(int CustomerId, string Name) : IRequest<Result>;

    public class UpdateCustomerNameHandler : IRequestHandler<UpdateCustomerNameCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerNameHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateCustomerNameCommand command, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.Name)) return Result.Failure("Tên không được để trống", 400);

                int rowsAffected = await _unitOfWork.CustomerRepository.UpdateCustomerProfileAsync(
                    command.CustomerId, 
                    command.Name, 
                    null, 
                    null, 
                    ct);

                if (rowsAffected == 0) return Result.Failure("Không tìm thấy người dùng", 404);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }

    public record UpdateCustomerPhoneCommand(int CustomerId, string PhoneNumber) : IRequest<Result>;

    public class UpdateCustomerPhoneHandler : IRequestHandler<UpdateCustomerPhoneCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerPhoneHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateCustomerPhoneCommand command, CancellationToken ct)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(command.PhoneNumber) || command.PhoneNumber.Length < 10) 
                    return Result.Failure("Số điện thoại không hợp lệ", 400);

                int rowsAffected = await _unitOfWork.CustomerRepository.UpdateCustomerProfileAsync(
                    command.CustomerId, 
                    null, 
                    command.PhoneNumber, 
                    null, 
                    ct);

                if (rowsAffected == 0) return Result.Failure("Không tìm thấy người dùng", 404);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }

    public record UpdateCustomerAddressCommand(int CustomerId, string Address) : IRequest<Result>;

    public class UpdateCustomerAddressHandler : IRequestHandler<UpdateCustomerAddressCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerAddressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateCustomerAddressCommand command, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.Address)) 
                    return Result.Failure("Địa chỉ không được để trống", 400);

                int rowsAffected = await _unitOfWork.CustomerRepository.UpdateCustomerProfileAsync(
                    command.CustomerId, 
                    null, 
                    null, 
                    command.Address, 
                    ct);

                if (rowsAffected == 0) return Result.Failure("Không tìm thấy người dùng", 404);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }

    public record UpdateCustomerPasswordCommand(int CustomerId, string OldPassword, string NewPassword) : IRequest<Result>;

    public class UpdateCustomerPasswordHandler : IRequestHandler<UpdateCustomerPasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppReadDbContext _db;

        public UpdateCustomerPasswordHandler(IUnitOfWork unitOfWork, IAppReadDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public async Task<Result> Handle(UpdateCustomerPasswordCommand command, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 6)
                    return Result.Failure("Mật khẩu mới phải có ít nhất 6 ký tự", 400);

                var customer = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_db.Customers, c => c.CustomerId == command.CustomerId, ct);
                
                if (customer == null) 
                    return Result.Failure("Không tìm thấy người dùng", 404);

                if (string.IsNullOrEmpty(customer.PasswordHash))
                {
                    return Result.Failure("Tài khoản chưa được thiết lập mật khẩu (có thể do đăng nhập bằng Google). Vui lòng thiết lập ở mục khác.", 400);
                }

                bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(command.OldPassword, customer.PasswordHash);
                if (!isOldPasswordValid)
                {
                    return Result.Failure("Mật khẩu cũ không chính xác", 400);
                }

                string newHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
                await _unitOfWork.CustomerRepository.UpdatePasswordAsync(command.CustomerId, newHash, ct);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }
}
