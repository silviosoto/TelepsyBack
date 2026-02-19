using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PsychologistController : ControllerBase
    {
        private readonly IPsychologistService _psychologistService;

        public PsychologistController(IPsychologistService psychologistService)
        {
            _psychologistService = psychologistService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var psychologist = await _psychologistService.GetPsychologistByIdAsync(id);
            if (psychologist == null) return NotFound();
            return Ok(psychologist);
        }

        [HttpGet("verified")]
        public async Task<IActionResult> GetVerified()
        {
            var psychologists = await _psychologistService.GetVerifiedPsychologistsAsync();
            return Ok(psychologists);
        }

        [HttpPut("{id}/profile")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] PsychologistProfileDto dto)
        {
            try
            {
                await _psychologistService.UpdateProfileAsync(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/upload-cv")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UploadCv(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            using (var stream = file.OpenReadStream())
            {
                await _psychologistService.UploadCvAsync(id, stream, file.FileName);
            }

            return Ok(new { message = "CV uploaded successfully" });
        }

        [HttpPost("{id}/upload-profile-picture")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            using (var stream = file.OpenReadStream())
            {
                await _psychologistService.UploadProfilePictureAsync(id, stream, file.FileName);
            }

            return Ok(new { message = "Profile picture uploaded successfully" });
        }
    }
}
