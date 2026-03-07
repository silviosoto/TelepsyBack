using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.Entities;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Psychologist")]
    public class PsychologyNotesController : ControllerBase
    {
        private readonly IPsychologyNoteService _noteService;
        private readonly IPsychologistService _psychologistService;

        public PsychologyNotesController(IPsychologyNoteService noteService, IPsychologistService psychologistService)
        {
            _noteService = noteService;
            _psychologistService = psychologistService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetNotesForPatient(int patientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var psych = await _psychologistService.GetPsychologistByUserIdAsync(userId);

                if (psych == null) return Unauthorized(new { message = "Invalid psychologist profile" });

                var notes = await _noteService.GetNotesForPatientAsync(patientId, psych.Id);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] PsychologyNote note)
        {
            try
            {
                var userId = GetCurrentUserId();
                var psych = await _psychologistService.GetPsychologistByUserIdAsync(userId);
                
                if (psych == null) return Unauthorized(new { message = "Invalid psychologist profile" });

                note.PsychologistId = psych.Id;
                note.Date = DateTime.UtcNow;

                await _noteService.CreateNoteAsync(note);

                return Ok(new { message = "Note created successfully", noteId = note.Id });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += " Inner: " + ex.InnerException.Message;

                return BadRequest(new { message = msg, stack = ex.StackTrace });
            }
        }

        [HttpPut("{noteId}")]
        public async Task<IActionResult> UpdateNote(int noteId, [FromBody] PsychologyNote note)
        {
            try
            {
                var userId = GetCurrentUserId();
                var psych = await _psychologistService.GetPsychologistByUserIdAsync(userId);
                
                if (psych == null) return Unauthorized(new { message = "Invalid psychologist profile" });

                if (noteId != note.Id) return BadRequest(new { message = "ID mismatch" });

                note.PsychologistId = psych.Id;

                await _noteService.UpdateNoteAsync(note);

                return Ok(new { message = "Note updated successfully" });
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
