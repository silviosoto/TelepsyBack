using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PsychologistController : ControllerBase
    {
        private readonly IPsychologistService _psychologistService;
        private readonly IAppointmentService _appointmentService;

        public PsychologistController(IPsychologistService psychologistService, IAppointmentService appointmentService)
        {
            _psychologistService = psychologistService;
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var psychologist = await _psychologistService.GetPsychologistByIdAsync(id);
            if (psychologist == null) return NotFound();
            return Ok(psychologist);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var psychologist = await _psychologistService.GetPsychologistByUserIdAsync(userId);
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

        [HttpGet("available-therapies")]
        public async Task<IActionResult> GetAvailableTherapies([FromQuery] string? query = null, [FromQuery] int? limit = null)
        {
            var therapies = await _psychologistService.GetAvailableTherapiesAsync(query, limit);
            return Ok(therapies);
        }

        [HttpGet("{id}/services")]
        public async Task<IActionResult> GetPsychologistServices(int id)
        {
            var services = await _psychologistService.GetPsychologistServicesAsync(id);
            return Ok(services);
        }

        [HttpPut("{id}/services")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UpdatePsychologistService(int id, [FromBody] UpdatePsychologistServiceDto dto)
        {
            try
            {
                await _psychologistService.UpdatePsychologistServiceAsync(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            try
            {
                var schedules = await _appointmentService.GetWorkScheduleAsync(id);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/schedule")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> SetSchedule(int id, [FromBody] List<WorkScheduleDto> schedules)
        {
            try
            {
                await _appointmentService.SetWorkScheduleAsync(id, schedules);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/patients")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> GetPatients(int id)
        {
            try
            {
                var patients = await _psychologistService.GetPatientsByPsychologistAsync(id);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("available-specialties")]
        public async Task<IActionResult> GetAvailableSpecialties([FromQuery] string? query = null, [FromQuery] int? limit = null)
        {
            var specialties = await _psychologistService.GetAvailableSpecialtiesAsync(query, limit);
            return Ok(specialties);
        }

        [HttpGet("{id}/specialties")]
        public async Task<IActionResult> GetPsychologistSpecialties(int id)
        {
            var specialties = await _psychologistService.GetPsychologistSpecialtiesAsync(id);
            return Ok(specialties);
        }

        [HttpPut("{id}/specialties")]
        [Authorize(Roles = "Psychologist")]
        public async Task<IActionResult> UpdatePsychologistSpecialty(int id, [FromBody] UpdatePsychologistSpecialtyDto dto)
        {
            try
            {
                await _psychologistService.UpdatePsychologistSpecialtyAsync(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
