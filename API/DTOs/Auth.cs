using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class ReqRegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Fullname { get; set; } = null!;
}
public class ReqLoginDTo
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
public class ReqGoogleLoginDTO
{
    public string IdToken { get; set; } = string.Empty;
}
