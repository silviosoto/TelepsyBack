using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelePsy.BLL.Interfaces;


namespace TelePsy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PsychologistsController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public PsychologistsController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpGet("match/{patientId}")]
        public async Task<IActionResult> GetMatches(int patientId)
        {
            // In a real app, we might get patientId from the current user claims to ensure security
            var matches = await _matchingService.GetMatchesForPatientAsync(patientId);
            return Ok(matches);
        }
    }
}
