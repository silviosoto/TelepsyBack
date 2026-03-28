using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly TelePsy.DAL.Repositories.IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, TelePsy.DAL.Repositories.IUnitOfWork unitOfWork, 
            IFileStorageService fileStorageService, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model, Microsoft.AspNetCore.Http.IFormFile? cvFile = null)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception("User already exists!");

            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    User user = new User()
                    {
                        Email = model.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = model.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"User creation failed! {errors}");
                    }

                    // Create Person
                    var person = new Person
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        UserId = user.Id,
                        IsActive = true,
                        Gender = "No especificado",
                        PhoneNumber = "Sin número",
                        Address = "Sin dirección",
                        City = "Sin ciudad",
                        State = "Sin departamento",
                        Country = "Colombia"
                    };

                    await _unitOfWork.Repository<Person>().AddAsync(person);
                    await _unitOfWork.CompleteAsync();

                    if (!await _roleManager.RoleExistsAsync(model.Role))
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));

                    await _userManager.AddToRoleAsync(user, model.Role);

                    if (model.Role == "Patient")
                    {
                        var patient = new Patient
                        {
                            PersonId = person.Id,
                            IsActive = true,
                            Occupation = "No especificada",
                            EmergencyContact = "No especificado",
                            PreferredGender = "No especificado",
                            Interests = ""
                        };
                        await _unitOfWork.Repository<Patient>().AddAsync(patient);
                    }
                    else if (model.Role == "Psychologist")
                    {
                        var psychologist = new Psychologist
                        {
                            PersonId = person.Id,
                            IsActive = true,
                            LicenseNumber = "Pendiente",
                            Specialization = "Pendiente",
                            University = "Pendiente",
                            Bio = "",
                            Hobbies = ""
                        };

                        if (cvFile != null)
                        {
                            // External IO before final DB commit
                            psychologist.CvPath = await _fileStorageService.SaveFileAsync(cvFile.OpenReadStream(), cvFile.FileName, "psychologist-cvs");
                        }

                        await _unitOfWork.Repository<Psychologist>().AddAsync(psychologist);
                    }

                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Non-critical operations after commit
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user, model.FirstName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending welcome email: {ex.Message}");
                    }

                    return await GenerateJwtToken(user);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return await GenerateJwtToken(user);
            }

            throw new Exception("Invalid credentials");
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto model)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                // Auto-register
                user = new User()
                {
                    Email = payload.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = payload.Email
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Google registration failed");

                // Create Person
                var person = new Person
                {
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    UserId = user.Id,
                    IsActive = true
                };
                await _unitOfWork.Repository<Person>().AddAsync(person);
                await _unitOfWork.CompleteAsync();

                // Default role? Or ask user to select role after google login?
                // For MVP, default to Patient
                if (!await _roleManager.RoleExistsAsync("Patient"))
                    await _roleManager.CreateAsync(new IdentityRole("Patient"));
                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient { PersonId = person.Id, IsActive = true };
                await _unitOfWork.Repository<Patient>().AddAsync(patient);
                await _unitOfWork.CompleteAsync();

                try
                {
                    await _emailService.SendWelcomeEmailAsync(user, payload.GivenName);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail registration
                    Console.WriteLine($"Error sending welcome email: {ex.Message}");
                }
            }

            return await GenerateJwtToken(user);
        }

        private async Task<AuthResponseDto> GenerateJwtToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var person = (await _unitOfWork.Repository<Person>().FindAsync(p => p.UserId == user.Id)).FirstOrDefault();

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FirstName", person?.FirstName ?? ""),
                new Claim("LastName", person?.LastName ?? "")
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(10),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Email = user.Email,
                Role = userRoles.Count > 0 ? userRoles[0] : null,
                FirstName = person?.FirstName ?? "",
                LastName = person?.LastName ?? ""
            };
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            // Security: Don't disclose if user exists
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Construct reset link
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
                
                await _emailService.SendPasswordResetEmailAsync(user, resetLink);
                return "Si el correo está registrado, recibirás un enlace de recuperación.";
            }

            return "Si el correo está registrado, recibirás un enlace de recuperación.";
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Password reset failed: {errors}");
            }

            return true;
        }
    }
}
