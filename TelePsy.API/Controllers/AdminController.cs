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
