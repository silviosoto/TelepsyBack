using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment appointment)
        {
            var result = await _appointmentService.CreateAppointmentAsync(appointment);
            return Ok(result);
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
