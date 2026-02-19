using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.Entities;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("patient/generate/{paymentId}")]
        public async Task<IActionResult> GeneratePatientInvoice(int paymentId)
        {
            try
            {
                var invoice = await _invoiceService.GeneratePatientInvoiceAsync(paymentId);
                return Ok(invoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpGet("psychologist/{psychologistId}/unpaid-sessions")]
        public async Task<IActionResult> GetUnpaidSessions(int psychologistId)
        {
            var sessions = await _invoiceService.GetUnpaidAppointmentsForPsychologistAsync(psychologistId);
            return Ok(sessions);
        }

        [HttpPost("psychologist/payout")]
        public async Task<IActionResult> GeneratePsychologistPayout([FromBody] PayoutRequestDto request)
        {
            try
            {
                var invoice = await _invoiceService.GeneratePsychologistPayoutAsync(request.PsychologistId, request.AppointmentIds);
                return Ok(invoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("admin/config/commission")]
        public async Task<IActionResult> UpdateCommission([FromBody] CommissionUpdateDto request)
        {
            await _invoiceService.UpdateGlobalCommissionAsync(request.Rate);
            return Ok();
        }

        [HttpGet("admin/config/commission")]
        public async Task<IActionResult> GetCommission()
        {
            var rate = await _invoiceService.GetGlobalCommissionAsync();
            return Ok(new { Rate = rate });
        }
    }

    public class PayoutRequestDto
    {
        public int PsychologistId { get; set; }
        public List<int> AppointmentIds { get; set; }
    }

    public class CommissionUpdateDto
    {
        public decimal Rate { get; set; }
    }
}
