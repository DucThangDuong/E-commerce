using Application.Common;
using Application.DTOs.Response;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using MassTransit;
using MediatR;

namespace Application.Features.Customers.Queries
{
    public record GetLoginUserQueries(string? Email, string? Password) : IRequest<Result<LoginResponse>>;

    public class GetLoginUserHandler : IRequestHandler<GetLoginUserQueries, Result<LoginResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJWTTokenServices _jwtTokenService;
        private readonly IPublishEndpoint _publishEndpoint;

        public GetLoginUserHandler(IUnitOfWork unitOfWork, IJWTTokenServices jwtTokenService, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<LoginResponse>> Handle(GetLoginUserQueries command, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password))
            {
                return Result<LoginResponse>.Failure("Email và mật khẩu không được để trống.", 400);
            }
            var userEntity = await _unitOfWork.CustomerRepository.GetByEmailAsync(command.Email);
            if (userEntity == null || !BCrypt.Net.BCrypt.Verify(command.Password, userEntity.PasswordHash))
            {
                return Result<LoginResponse>.Failure("Tài khoản hoặc mật khẩu không chính xác.", 401);
            }
            try
            {
                string role = userEntity.Role ?? "User";
                var accessToken = _jwtTokenService.GenerateAccessToken(userEntity.CustomerId, userEntity.Email!, role);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                userEntity.RefreshToken = refreshToken.Token;
                userEntity.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
                userEntity.LoginProvider = "Custom";

                await _publishEndpoint.Publish(new Application.DTOs.Services.SendMail(command.Email!,
                    "Đăng nhập thành công", $"Bạn đã đăng nhập thành công vào tài khoản của mình vào lúc {DateTime.UtcNow}. " +
                    $"Nếu đây không phải là bạn, vui lòng liên hệ với bộ phận hỗ trợ ngay lập tức."), ct);

                await _unitOfWork.SaveChangesAsync(ct);

                return Result<LoginResponse>.Success(new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiryTime = refreshToken.ExpiryDate,
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResponse>.Failure($"Lỗi server: {ex.Message}", 500);
            }
        }
    }
}
