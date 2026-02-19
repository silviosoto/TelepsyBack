using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/psychologist/[controller]")] // e.g. api/psychologist/therapies ? No, Route matches class name by default.
    // Let's use a more RESTful path. api/psychologist/therapies
    // But the class is PsychologistTherapyController.
    // I'll stick to explicit attribute routing on methods or base route.
    [Route("api/psychologist/therapies")]
    [Authorize(Roles = "Psychologist")]
    public class PsychologistTherapyController : ControllerBase
    {
        private readonly IPsychologistTherapyService _psychologistTherapyService;
        private readonly IPsychologistService _psychologistService;

        public PsychologistTherapyController(IPsychologistTherapyService psychologistTherapyService, IPsychologistService psychologistService)
        {
            _psychologistTherapyService = psychologistTherapyService;
            _psychologistService = psychologistService;
        }

        private async Task<int> GetCurrentPsychologistIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Default claim for ID in Identity is NameIdentifier
            
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token");

            var psychologist = await _psychologistService.GetPsychologistByUserIdAsync(userId);
            if (psychologist == null)
                throw new Exception("Psychologist profile not found for current user");

            return psychologist.Id;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTherapies()
        {
            try
            {
                var psychologistId = await GetCurrentPsychologistIdAsync();
                var therapies = await _psychologistTherapyService.GetByPsychologistIdAsync(psychologistId);
                return Ok(therapies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetRate([FromBody] SetRateDto dto)
        {
            try
            {
                var psychologistId = await GetCurrentPsychologistIdAsync();
                await _psychologistTherapyService.SetRateAsync(psychologistId, dto);
                return Ok(new { message = "Rate updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{therapyId}")]
        public async Task<IActionResult> RemoveTherapy(int therapyId)
        {
            try
            {
                var psychologistId = await GetCurrentPsychologistIdAsync();
                await _psychologistTherapyService.RemoveTherapyAsync(psychologistId, therapyId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
