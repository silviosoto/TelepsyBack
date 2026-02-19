using FluentValidation;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Role).NotEmpty().Must(x => x == "Patient" || x == "Psychologist").WithMessage("Role must be Patient or Psychologist.");
        }
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        }
    }
}
