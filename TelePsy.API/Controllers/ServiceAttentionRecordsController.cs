using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Psychologist,Admin")]
    public class ServiceAttentionRecordsController : ControllerBase
    {
        private readonly IServiceAttentionRecordService _recordService;
        private readonly IPsychologistService _psychologistService;

        public ServiceAttentionRecordsController(
            IServiceAttentionRecordService recordService,
            IPsychologistService psychologistService)
        {
            _recordService = recordService;
            _psychologistService = psychologistService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetForPatient(int patientId)
        {
            try
            {
                var records = await _recordService.GetRecordsForPatientAsync(patientId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("active/{patientId}/{therapyId}")]
        public async Task<IActionResult> GetActiveForService(int patientId, int therapyId)
        {
            try
            {
                var record = await _recordService.GetActiveRecordForServiceAsync(patientId, therapyId);
                if (record == null)
                    return Ok(new { exists = false });

                return Ok(new { exists = true, record });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var record = await _recordService.GetByIdAsync(id);
                if (record == null)
                    return NotFound(new { message = "Registro de atención no encontrado." });

                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceAttentionRecordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var psych = await _psychologistService.GetPsychologistByUserIdAsync(userId);
                if (psych == null) return Unauthorized(new { message = "Perfil de psicólogo inválido." });

                var created = await _recordService.CreateRecordAsync(dto, psych.Id);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceAttentionRecordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var psych = await _psychologistService.GetPsychologistByUserIdAsync(userId);
                if (psych == null) return Unauthorized(new { message = "Perfil de psicólogo inválido." });

                var updated = await _recordService.UpdateRecordAsync(id, dto, psych.Id);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
