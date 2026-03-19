using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.IServices;
using MediatR;

namespace Application.Features.Customers.Queries
{
    public record RefreshTokenCommand(string? RefreshToken) : IRequest<Result<LoginResponse>>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJWTTokenServices _jwtTokenService;

        public RefreshTokenHandler(IUnitOfWork unitOfWork, IJWTTokenServices jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand command, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(command.RefreshToken))
            {
                return Result<LoginResponse>.Failure("Không tìm thấy Refresh Token.", 400);
            }

            var user = await _unitOfWork.CustomerRepository.GetUserByRefreshTokenAsync(command.RefreshToken);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Token không hợp lệ.", 401);
            }

            if (user.RefreshToken != command.RefreshToken)
            {
                return Result<LoginResponse>.Failure("Token không khớp.", 401);
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<LoginResponse>.Failure("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", 401);
            }

            var newAccessToken = _jwtTokenService.GenerateAccessToken(user.CustomerId, user.Email!, user.Role!);

            return Result<LoginResponse>.Success(new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime ?? DateTime.UtcNow
            });
        }
    }
}
