using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, TelePsy.DAL.Repositories.IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception("User already exists!");

            User user = new User()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception("User creation failed! Please check user details and try again.");

            // Create Person
            var person = new Person
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserId = user.Id,
                IsActive = true
            };

            await _unitOfWork.Repository<Person>().AddAsync(person);
            await _unitOfWork.CompleteAsync();

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            await _userManager.AddToRoleAsync(user, model.Role);

            // Create specific entity based on role if needed (Patient/Psychologist specific tables)
            // Ideally we should link Patient/Psychologist to Person here.

            if (model.Role == "Patient")
            {
                var patient = new Patient { PersonId = person.Id, IsActive = true };
                await _unitOfWork.Repository<Patient>().AddAsync(patient);
            }
            else if (model.Role == "Psychologist")
            {
                var psychologist = new Psychologist { PersonId = person.Id, IsActive = true };
                await _unitOfWork.Repository<Psychologist>().AddAsync(psychologist);
            }

            await _unitOfWork.CompleteAsync();

            return await GenerateJwtToken(user);
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
            }

            return await GenerateJwtToken(user);
        }

        private async Task<AuthResponseDto> GenerateJwtToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
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
                Role = userRoles.Count > 0 ? userRoles[0] : null
            };
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token; // In a real app, send this via email
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
