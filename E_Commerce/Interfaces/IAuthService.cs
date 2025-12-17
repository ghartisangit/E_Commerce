using E_Commerce.DTOs;

namespace E_Commerce.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(int userId, string email, string role);
    }
}
