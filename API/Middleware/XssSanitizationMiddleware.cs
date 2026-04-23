using Ganss.Xss;
using System.Text;
using System.Text.Json;

namespace API.Middleware
{
    public class XssSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HtmlSanitizer _sanitizer;

        public XssSanitizationMiddleware(RequestDelegate next)
        {
            _next = next;
            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedCssProperties.Clear();
            _sanitizer.AllowedSchemes.Clear();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsJsonContentType(context.Request) && HasBody(context.Request))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        using var document = JsonDocument.Parse(body);
                        var sanitizedJson = SanitizeJsonElement(document.RootElement);
                        string sanitizedBody = JsonSerializer.Serialize(sanitizedJson);

                        var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                        context.Request.Body = new MemoryStream(sanitizedBytes);
                        context.Request.ContentLength = sanitizedBytes.Length;
                    }
                    catch (JsonException)
                    {
                        context.Request.Body.Position = 0;
                    }
                }
                else
                {
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }

        private object? SanitizeJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();
                    foreach (var property in element.EnumerateObject())
                    {
                        obj[property.Name] = SanitizeJsonElement(property.Value);
                    }
                    return obj;

                case JsonValueKind.Array:
                    var arr = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        arr.Add(SanitizeJsonElement(item));
                    }
                    return arr;

                case JsonValueKind.String:
                    string rawValue = element.GetString() ?? string.Empty;
                    return _sanitizer.Sanitize(rawValue);

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longVal)) return longVal;
                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                default:
                    return null;
            }
        }

        private static bool IsJsonContentType(HttpRequest request)
        {
            return request.ContentType != null &&
                   request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasBody(HttpRequest request)
        {
            return request.ContentLength > 0 ||
                   (request.Method == HttpMethods.Post ||
                    request.Method == HttpMethods.Put ||
                    request.Method == HttpMethods.Patch);
        }
    }
}
