using System.ComponentModel.DataAnnotations;

namespace TelePsy.Domain.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Role { get; set; } // Patient, Psychologist
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class GoogleLoginDto
    {
        [Required]
        public string IdToken { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
