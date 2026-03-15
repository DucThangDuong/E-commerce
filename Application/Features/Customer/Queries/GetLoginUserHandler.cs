using Application.Common;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using MassTransit;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Customer.Queries
{
    public record LoginCommand(string ?Email, string ?Password);

    public class GetLoginUserHandler : IQueryHandler<LoginCommand,int>
    {
        private readonly IUnitOfWork _context;
        private readonly IJWTTokenServices _jwtTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublishEndpoint _publishEndpoint;

        public GetLoginUserHandler(IUnitOfWork context, IJWTTokenServices jwtTokenService, IHttpContextAccessor httpContextAccessor, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _httpContextAccessor = httpContextAccessor;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<int>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password))
            {
                return Result<int>.Failure("Email và mật khẩu không được để trống.", 400);
            }
            var userEntity = await _context.CustomerRepository.GetByEmailAsync(command.Email);
            if (userEntity == null || !BCrypt.Net.BCrypt.Verify(command.Password, userEntity.PasswordHash))
            {
                return Result<int>.Failure("Tài khoản hoặc mật khẩu không chính xác.", 401);
            }
            try
            {
                string role = userEntity.Role ?? "User";
                var accessToken = _jwtTokenService.GenerateAccessToken(userEntity.CustomerId, userEntity.Email!, role);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                userEntity.RefreshToken = refreshToken.Token;
                userEntity.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
                userEntity.LoginProvider = "Custom";

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = refreshToken.ExpiryDate,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        IsEssential = true
                    });
                }

                await _publishEndpoint.Publish(new SendMail(command.Email!, 
                    "Đăng nhập thành công", $"Bạn đã đăng nhập thành công vào tài khoản của mình vào lúc {DateTime.UtcNow}. " +
                    $"Nếu đây không phải là bạn, vui lòng liên hệ với bộ phận hỗ trợ ngay lập tức."));
                await _context.SaveChangesAsync();
                return Result<int>.Success(1);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Lỗi server: {ex.Message}", 500);
            }
        }
    }
}
