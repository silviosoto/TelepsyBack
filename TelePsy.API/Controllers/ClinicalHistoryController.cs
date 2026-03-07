using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.Entities;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Psychologist")]
    public class ClinicalHistoryController : ControllerBase
    {
        private readonly IClinicalRecordService _clinicalRecordService;
        private readonly IPsychologistService _psychologistService;

        public ClinicalHistoryController(IClinicalRecordService clinicalRecordService, IPsychologistService psychologistService)
        {
            _clinicalRecordService = clinicalRecordService;
            _psychologistService = psychologistService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetPatientHistory(int patientId)
        {
            try
            {
                var records = await _clinicalRecordService.GetRecordsForPatientAsync(patientId);
                var latestRecord = records.OrderByDescending(r => r.Date).FirstOrDefault();

                if (latestRecord == null)
                {
                    // Return empty JSON object to signify no history yet
                    return Ok(new { });
                }

                // Parse the Notes field which we use to store all the extra JSON data
                var data = string.IsNullOrEmpty(latestRecord.Notes) ? new object() : JsonSerializer.Deserialize<object>(latestRecord.Notes);
                
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{patientId}")]
        public async Task<IActionResult> UpdatePatientHistory(int patientId, [FromBody] JsonElement data)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var psychologist = await _psychologistService.GetPsychologistByUserIdAsync(userId);
                if (psychologist == null) return Unauthorized();

                // Make sure to parse out what we can for Diagnosis and TreatmentPlan to keep the model relevant
                string diagnosis = "";
                string treatmentPlan = "";

                if (data.TryGetProperty("mainDiagnosis", out var diagElem))
                    diagnosis = diagElem.GetString() ?? "";

                if (data.TryGetProperty("treatmentPlan", out var treatElem))
                    treatmentPlan = treatElem.GetString() ?? "";

                var newRecord = new ClinicalRecord
                {
                    PatientId = patientId,
                    PsychologistId = psychologist.Id,
                    Date = DateTime.UtcNow,
                    Diagnosis = diagnosis,
                    TreatmentPlan = treatmentPlan,
                    Notes = JsonSerializer.Serialize(data) // Save the whole JSON object
                };

                await _clinicalRecordService.CreateRecordAsync(newRecord);

                return Ok();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += " Inner: " + ex.InnerException.Message;

                return BadRequest(new { message = msg, stack = ex.StackTrace });
            }
        }
    }
}
