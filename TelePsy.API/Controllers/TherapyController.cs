using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.DTOs;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TherapyController : ControllerBase
    {
        private readonly ITherapyService _therapyService;

        public TherapyController(ITherapyService therapyService)
        {
            _therapyService = therapyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var therapies = await _therapyService.GetAllAsync();
            return Ok(therapies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var therapy = await _therapyService.GetByIdAsync(id);
            if (therapy == null) return NotFound();
            return Ok(therapy);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTherapyDto dto)
        {
            try
            {
                var therapy = await _therapyService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = therapy.Id }, therapy);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTherapyDto dto)
        {
            try
            {
                await _therapyService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _therapyService.ToggleStatusAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
