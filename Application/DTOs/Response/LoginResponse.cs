namespace Application.DTOs.Response;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; }
}
