using Application.DTOs;
using Application.DTOs.Services;
namespace Application.IServices
{
    public interface IGoogleAuthService
    {
        Task<AuthResultDTO> HandleGoogleLoginAsync(string idToken);
    }
}
