using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Services;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
public class AuthResultDTO
{
    public bool IsSuccess { get; set; }
    public string? CustomJwtToken { get; set; }
    public string? ErrorMessage { get; set; }
    public RefreshToken refreshToken { get; set; } = null!;
    public string? Email { get; set; }
}
public enum StorageType
{
    product
}

