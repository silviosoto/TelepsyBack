using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment appointment)
        {
            var result = await _appointmentService.CreateAppointmentAsync(appointment);
            return Ok(result);
        }

        [HttpPost("initiate")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Initiate([FromBody] InitiateBookingDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var result = await _appointmentService.InitiateBookingAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("checkout-summary/{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetCheckoutSummary(int id)
        {
            try
            {
                var result = await _appointmentService.GetCheckoutSummaryAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-appointments")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var appointments = await _appointmentService.GetPatientAppointmentsByUserIdAsync(userId);
                
                var dtos = appointments.Select(a => new PatientAppointmentDto
                {
                    Id = a.Id,
                    PsychologistId = a.PsychologistId,
                    PsychologistName = $"{a.Psychologist?.Person?.FirstName} {a.Psychologist?.Person?.LastName}",
                    PsychologistSpecialty = a.Psychologist?.Specialization ?? "General",
                    TherapyId = a.TherapyId,
                    TherapyName = a.Therapy.Name ?? "Sesión de Psicología",
                    ScheduledTime = a.ScheduledTime,
                    DurationMinutes = a.DurationMinutes,
                    Status = (int)a.Status,
                    VideoLink = a.VideoLink,
                    IsPackage = a.SessionPackageId.HasValue,
                    SessionPackageId = a.SessionPackageId
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-packages")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyPackages()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var result = await _appointmentService.GetActivePackagesForPatientAsync(userId);
                
                // Construct a simpler DTO to prevent JSON cycle issues and unneeded data
                var packageDtos = result.Select(p => new
                {
                    p.Id,
                    p.PsychologistId,
                    PsychologistName = $"{p.Psychologist?.Person?.FirstName} {p.Psychologist?.Person?.LastName}",
                    p.TherapyId,
                    TherapyName = p.Therapy?.Name,
                    p.TotalSessions,
                    p.UsedSessions,
                    p.IsActive,
                    p.CreatedAt
                });

                return Ok(packageDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetForPatient(int patientId)
        {
            var result = await _appointmentService.GetAppointmentsForPatientAsync(patientId);
            return Ok(result);
        }

        [Authorize(Roles = "Psychologist")]
        [HttpGet("psychologist/{psychologistId}")]
        public async Task<IActionResult> GetForPsychologist(int psychologistId)
        {
            var result = await _appointmentService.GetAppointmentsForPsychologistAsync(psychologistId);
            return Ok(result);
        }

        [HttpGet("available-slots/{psychologistId}")]
        public async Task<IActionResult> GetAvailableSlots(int psychologistId, [FromQuery] DateTime date, [FromQuery] int durationMinutes = 45)
        {
            try
            {
                var result = await _appointmentService.GetAvailableSlotsAsync(psychologistId, date, durationMinutes);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentService.CancelAppointmentAsync(id);
            return Ok();
        }

        [HttpPut("reschedule/{id}")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDto dto)
        {
            try
            {
                await _appointmentService.RescheduleAppointmentAsync(id, dto.NewDate);
                return Ok(new { message = "Cita reprogramada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var role = User.FindFirstValue(ClaimTypes.Role);
                var link = await _appointmentService.JoinAppointmentAsync(id, userId, role ?? "");
                
                return Ok(new { link });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RescheduleDto 
    {
        public DateTime NewDate { get; set; }
    }
}
