using Application.DTOs;
using Application.DTOs.Services;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace API.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJWTTokenServices _jwtTokenService;
        private readonly IUnitOfWork _context;

        public GoogleAuthService(IConfiguration configuration, IJWTTokenServices jwtTokenService, IUnitOfWork context)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }


        public async Task<AuthResultDTO> HandleGoogleLoginAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { googleClientId ?? "" }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = "Token Google không h?p l? ho?c dã h?t h?n." };
            }
            try
            {
                string email = payload.Email;
                string name = payload.Name;
                string picture = payload.Picture;
                string googleId = payload.Subject;
                var customer = await _context.CustomerRepository.GetByEmailAsync(email);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = email.Split('@')[0],
                        Email = email,
                        PasswordHash = "",
                        GoogleAvatar = picture,
                        GoogleId = googleId,
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        RefreshToken = refreshToken.Token,
                        RefreshTokenExpiryTime = refreshToken.ExpiryDate,
                        LoginProvider = "Google",
                    };
                    await _context.CustomerRepository.AddAsync(customer);
                }
                else
                {
                    if (string.IsNullOrEmpty(customer.GoogleId))
                    {
                        customer.GoogleId = googleId;
                    }
                    if (string.IsNullOrEmpty(customer.GoogleAvatar))
                    {
                        customer.GoogleAvatar = picture;
                    }
                    customer.RefreshToken = refreshToken.Token;
                    customer.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
                    customer.LoginProvider = "Google";
                }
                await _context.SaveChangesAsync();

                string customJwtToken = _jwtTokenService.GenerateAccessToken(customer.CustomerId, email, customer.Role!);
                return new AuthResultDTO
                {
                    IsSuccess = true,
                    CustomJwtToken = customJwtToken,
                    refreshToken = refreshToken,
                    Email=email
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = $"Ðã x?y ra l?i trong quá trình x? lý dang nh?p Google: {ex.Message}" };
            }
        }
    }
}

