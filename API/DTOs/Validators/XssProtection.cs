using System.Text.RegularExpressions;
namespace API.DTOs.Validators;
public static partial class XssProtection
{

    [GeneratedRegex(@"<[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"\bon\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EventHandlerPattern();


    [GeneratedRegex(@"(javascript|vbscript|data)\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DangerousProtocolPattern();


    [GeneratedRegex(@"&#(x[0-9a-fA-F]+|\d+);?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex HtmlEntityPattern();

    [GeneratedRegex(@"expression\s*\(|url\s*\(\s*(javascript|data)\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CssInjectionPattern();


    [GeneratedRegex(@"[<>""'`]", RegexOptions.Compiled)]
    private static partial Regex DangerousCharsPattern();

    public static bool IsCleanText(string? value)
    {
        if (string.IsNullOrEmpty(value)) return true;

        return !HtmlTagPattern().IsMatch(value)
            && !EventHandlerPattern().IsMatch(value)
            && !DangerousProtocolPattern().IsMatch(value)
            && !HtmlEntityPattern().IsMatch(value)
            && !CssInjectionPattern().IsMatch(value)
            && !DangerousCharsPattern().IsMatch(value);
    }
    public static bool IsCleanDescription(string? value)
    {
        if (string.IsNullOrEmpty(value)) return true;

        return !HtmlTagPattern().IsMatch(value)
            && !EventHandlerPattern().IsMatch(value)
            && !DangerousProtocolPattern().IsMatch(value)
            && !HtmlEntityPattern().IsMatch(value)
            && !CssInjectionPattern().IsMatch(value);
    }

    public static bool IsCleanUrl(string? value)
    {
        if (string.IsNullOrEmpty(value)) return true;

        if (DangerousProtocolPattern().IsMatch(value)) return false;

        if (HtmlTagPattern().IsMatch(value)) return false;
        if (EventHandlerPattern().IsMatch(value)) return false;

        return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}
