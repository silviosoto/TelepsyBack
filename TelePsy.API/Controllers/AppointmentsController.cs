using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
                if (result == null) return NotFound();
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

                var result = await _appointmentService.GetPatientAppointmentsByUserIdAsync(userId);
                return Ok(result);
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

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentService.CancelAppointmentAsync(id);
            return Ok();
        }
    }
}
