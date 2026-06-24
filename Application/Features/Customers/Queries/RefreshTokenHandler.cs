using Application.Common;
using Application.DTOs.Response;
using Application.IServices;
using MediatR;
using StackExchange.Redis;
using System.Security.Claims;

namespace Application.Features.Customers.Queries
{
    public record RefreshTokenCommand(string? AccessToken, string? RefreshToken) : IRequest<Result<LoginResponse>>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
    {
        private readonly IJWTTokenServices _jwtTokenService;
        private readonly IDatabase _redisConnection;

        public RefreshTokenHandler(IJWTTokenServices jwtTokenService, IConnectionMultiplexer multiplexer)
        {
            _jwtTokenService = jwtTokenService;
            _redisConnection = multiplexer.GetDatabase();
        }

        public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand command, CancellationToken ct)
        {
            try
            {

                if (string.IsNullOrEmpty(command.RefreshToken) || string.IsNullOrEmpty(command.AccessToken))
                {
                    return Result<LoginResponse>.Failure("Không tìm thấy Token.", 400);
                }
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(command.AccessToken);
                string? userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {   
                    return Result<LoginResponse>.Failure("Token không hợp lệ.", 401);
                }
                int userId = int.Parse(userIdString);
                string redisKey = $"RefreshToken:{userId}";
                string? refreshToken = await _redisConnection.StringGetAsync(redisKey);
                if (string.IsNullOrEmpty(refreshToken) || refreshToken != command.RefreshToken)
                {
                    return Result<LoginResponse>.Failure("Token không hợp lệ.", 401);
                }

                var newAccessToken = _jwtTokenService.GenerateAccessToken(userId, Domain.Enums.UserRole.User.ToString());
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
                await _redisConnection.KeyDeleteAsync(redisKey);
                await _redisConnection.StringSetAsync(redisKey, newRefreshToken.Token, TimeSpan.FromDays(7));
                return Result<LoginResponse>.Success(new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    RefreshTokenExpiryTime = newRefreshToken.ExpiryDate
                },201);
            }
            catch
            {
                return Result<LoginResponse>.Failure("Loi server", 500);
            }
        }
    }
}
