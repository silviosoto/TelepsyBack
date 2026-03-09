using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;

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
    }

    public class RejectDto
    {
        public string Reason { get; set; }
    }

    public class CommissionDto
    {
        public decimal Rate { get; set; }
    }
}
