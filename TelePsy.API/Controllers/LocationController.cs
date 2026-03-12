using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelePsy.DAL.Context;
using TelePsy.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            return await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        [HttpGet("departments/{departmentId}/cities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities(int departmentId)
        {
            return await _context.Cities
                .Where(c => c.DepartmentId == departmentId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
