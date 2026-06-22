using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (!requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Handling {RequestName} with payload {@Request}", requestName, request);
            }
            else
            {
                _logger.LogInformation("Handling {RequestName} (Payload logging disabled for queries)", requestName);
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await next();
                stopwatch.Stop();
                
                if (stopwatch.ElapsedMilliseconds > 500)
                {
                    _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} ms)",
                        requestName, stopwatch.ElapsedMilliseconds);
                }

                _logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error Handling {RequestName} after {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
