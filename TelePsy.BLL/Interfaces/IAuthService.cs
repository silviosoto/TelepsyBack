using System.Threading.Tasks;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model, Microsoft.AspNetCore.Http.IFormFile? cvFile = null);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto model);
        Task<string> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto model);
    }
}
