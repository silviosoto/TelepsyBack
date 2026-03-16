using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;
using System.Linq;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("psychologists/pending")]
        public async Task<IActionResult> GetPendingPsychologists()
        {
            var psychologists = await _adminService.GetPendingPsychologistsAsync();
            return Ok(psychologists);
        }

        [HttpPost("psychologists/{id}/approve")]
        public async Task<IActionResult> ApprovePsychologist(int id)
        {
            try
            {
                await _adminService.ApprovePsychologistAsync(id);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("psychologists/{id}/reject")]
        public async Task<IActionResult> RejectPsychologist(int id, [FromBody] RejectDto dto)
        {
            try
            {
                await _adminService.RejectPsychologistAsync(id, dto.Reason);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("commission")]
        public async Task<IActionResult> GetCommission()
        {
            var rate = await _adminService.GetCommissionRateAsync();
            return Ok(new { Rate = rate });
        }

        [HttpPut("commission")]
        public async Task<IActionResult> UpdateCommission([FromBody] CommissionDto dto)
        {
            await _adminService.UpdateCommissionRateAsync(dto.Rate);
            return Ok();
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] System.DateTime? date = null)
        {
            try
            {
                var result = await _adminService.GetPatientsAsync(page, pageSize, search, date);
                
                var response = new 
                {
                    Data = result.Patients.Select(p => new
                    {
                        p.Id,
                        FullName = $"{p.Person?.FirstName} {p.Person?.LastName}",
                        Email = p.Person?.User?.Email,
                        Phone = p.Person?.PhoneNumber,
                        CreatedAt = p.Person?.User?.CreatedAt
                    }),
                    TotalCount = result.TotalCount,
                    Page = page,
                    PageSize = pageSize
                };
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("psychologists")]
        public async Task<IActionResult> GetPsychologists([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] bool? isVerified = null, [FromQuery] System.DateTime? date = null)
        {
            try
            {
                var result = await _adminService.GetPsychologistsAsync(page, pageSize, search, isVerified, date);
                
                var response = new 
                {
                    Data = result.Psychologists.Select(p => new
                    {
                        p.Id,
                        FullName = $"{p.Person?.FirstName} {p.Person?.LastName}",
                        Email = p.Person?.User?.Email,
                        Phone = p.Person?.PhoneNumber,
                        p.Specialization,
                        p.IsVerified,
                        p.IsActive,
                        CreatedAt = p.Person?.User?.CreatedAt
                    }),
                    TotalCount = result.TotalCount,
                    Page = page,
                    PageSize = pageSize
                };
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("psychologists/{id}")]
        public async Task<IActionResult> GetPsychologistDetails(int id)
        {
            try
            {
                var p = await _adminService.GetPsychologistDetailsAsync(id);
                if (p == null) return NotFound();

                return Ok(new
                {
                    p.Id,
                    FullName = $"{p.Person?.FirstName} {p.Person?.LastName}",
                    Email = p.Person?.User?.Email,
                    Phone = p.Person?.PhoneNumber,
                    Address = p.Person?.Address,
                    City = p.Person?.City,
                    p.Specialization,
                    p.LicenseNumber,
                    p.University,
                    p.ExperienceYears,
                    p.Bio,
                    p.Hobbies,
                    p.CvPath,
                    p.IsVerified,
                    p.IsActive,
                    CreatedAt = p.Person?.User?.CreatedAt,
                    Specialties = p.Specialties?.Select(s => s.Specialty?.Name),
                    Therapies = p.Therapies?.Select(t => new { t.Therapy?.Name, t.Rate })
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("psychologists/{id}/appointments")]
        public async Task<IActionResult> GetPsychologistAppointments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var (appointments, totalCount) = await _adminService.GetPsychologistAppointmentsAsync(id, page, pageSize, searchTerm, startDate, endDate);
                return Ok(new
                {
                    Data = appointments.Select(a => new
                    {
                        a.Id,
                        PatientName = $"{a.Patient?.Person?.FirstName} {a.Patient?.Person?.LastName}",
                        TherapyName = a.Therapy?.Name,
                        a.ScheduledTime,
                        a.Status,
                        a.Rate
                    }),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("psychologists/{id}/payments")]
        public async Task<IActionResult> GetPsychologistPayments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] string? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var (payments, totalCount) = await _adminService.GetPsychologistPaymentsAsync(id, page, pageSize, searchTerm, status, startDate, endDate);
                return Ok(new
                {
                    Data = payments.Select(p => new
                    {
                        p.Id,
                        p.Amount,
                        p.Date,
                        p.Status,
                        p.TransactionId,
                        PatientName = $"{p.Appointment?.Patient?.Person?.FirstName} {p.Appointment?.Patient?.Person?.LastName}",
                        AppointmentId = p.AppointmentId
                    }),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPaymentManagement([FromQuery] int? psychologistId = null, [FromQuery] int? patientId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var payments = await _adminService.GetPaymentManagementAsync(psychologistId, patientId, startDate, endDate);
                return Ok(payments);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("payments/payout")]
        public async Task<IActionResult> ProcessPsychologistPayout([FromBody] PsychologistPayoutRequestDto request)
        {
            try
            {
                await _adminService.ProcessPsychologistPayoutAsync(request);
                return Ok(new { Message = "Payout processed successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class RejectDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CommissionDto
    {
        public decimal Rate { get; set; }
    }
}
