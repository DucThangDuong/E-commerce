using Application.Common;
using MediatR;
using StackExchange.Redis;

namespace Application.Features.Customers.Commands
{
    public record UpdateRevokeRefreshTokenCommand(int customerId,string? accessToken) : IRequest<Result>;
    public class UpdateRevokeRefreshTokenHandler : IRequestHandler<UpdateRevokeRefreshTokenCommand, Result>
    {
        private readonly IDatabase _redisConnection;
        public UpdateRevokeRefreshTokenHandler(IConnectionMultiplexer multiplexer)
        {
            _redisConnection = multiplexer.GetDatabase();
        }

        public async Task<Result> Handle(UpdateRevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string redisKey = $"RefreshToken:{request.customerId}";
                await _redisConnection.KeyDeleteAsync(redisKey);
                if (!string.IsNullOrEmpty(request.accessToken))
                {
                    await _redisConnection.StringSetAsync($"Blacklist:{request.accessToken}", "banned", TimeSpan.FromMinutes(15));
                }
                return Result.Success(204);
            }
            catch (Exception ex)
            {
                return Result.Failure("Lỗi server: " + ex.Message, 500);
            }
        }
    }
}
