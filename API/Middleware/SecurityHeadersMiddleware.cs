namespace API.Middleware
{
    /// <summary>
    /// Middleware thêm các HTTP Security Headers để bảo vệ chống XSS, clickjacking, MIME sniffing, v.v.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Ngăn trình duyệt render HTML trong response — chống reflected XSS
            headers["X-Content-Type-Options"] = "nosniff";

            // Ngăn trang bị nhúng trong iframe — chống clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Bật XSS filter của trình duyệt (legacy nhưng vẫn hữu ích cho trình duyệt cũ)
            headers["X-XSS-Protection"] = "1; mode=block";

            // Không gửi referrer khi navigate cross-origin
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Chỉ cho phép HTTPS (bật khi deploy production có SSL)
            // headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            // Content Security Policy — chặn inline scripts và chỉ cho phép tài nguyên từ cùng origin
            headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' https://fonts.gstatic.com;";

            // Không cho phép trình duyệt cache response chứa dữ liệu nhạy cảm
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";

            await _next(context);
        }
    }
}
