using API.DTOs;
using Application.Common;
using FastEndpoints;

namespace API.Extensions
{
    public static class EndpointExtensions
    {
        public static async Task SendApiResponseAsync<TData>(
            this BaseEndpoint ep,
            Result<TData> result,
            CancellationToken ct,
            string Message = "Thực hiện thành công",
            string ErrorCode = "ERR_BAD_REQUEST")
        {
            if (result.IsSuccess)
            {
                var response = new ApiSuccessResponse<TData>
                {
                    Message = Message,
                    Data = result.Data,
                };
                await ep.HttpContext.Response.SendAsync(response, result.StatusCode, cancellation: ct);
            }
            else
            {
                var response = new ApiErrorResponse
                {
                    Message = result.ErrorCode ?? "Đã xảy ra lỗi",
                    ErrorCode = result.StatusCode == 400 || result.StatusCode == 409 ? ErrorCode : "ERR_INTERNAL_SERVER",
                    Errors = result.Errors != null && result.Errors.Any() ? result.Errors : result.Data,
                    TraceId = ep.HttpContext.TraceIdentifier
                };
                await ep.HttpContext.Response.SendAsync(response, result.StatusCode, cancellation: ct);
            }
        }

        public static async Task SendApiResponseAsync(
            this BaseEndpoint ep, 
            Result result, 
            CancellationToken ct,
            string Message = "Thực hiện thành công",
            string defaultErrorCode = "ERR_BAD_REQUEST")
        {
            if (result.IsSuccess)
            {
                var response = new ApiSuccessResponse<object>
                {
                    Message = Message,
                };
                await ep.HttpContext.Response.SendAsync(response, result.StatusCode, cancellation: ct);
            }
            else
            {
                var response = new ApiErrorResponse
                {
                    Message = result.ErrorCode ?? "Đã xảy ra lỗi",
                    ErrorCode = result.StatusCode == 400 || result.StatusCode == 409 ? defaultErrorCode : "ERR_INTERNAL_SERVER",
                    Errors = result.Errors != null && result.Errors.Any() ? result.Errors : null,
                    TraceId = ep.HttpContext.TraceIdentifier
                };
                await ep.HttpContext.Response.SendAsync(response, result.StatusCode, cancellation: ct);
            }
        }
    }
}
