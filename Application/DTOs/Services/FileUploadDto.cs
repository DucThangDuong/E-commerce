namespace Application.DTOs.Services;

/// <summary>
/// Abstraction for file upload data, replacing direct dependency on ASP.NET Core's IFormFile.
/// </summary>
public class FileUploadDto
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
