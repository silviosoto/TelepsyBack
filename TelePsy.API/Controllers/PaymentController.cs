using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelePsy.BLL.Interfaces;

namespace TelePsy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("checkout/{invoiceId}")]
        [Authorize]
        public async Task<IActionResult> GetCheckoutData(int invoiceId)
        {
            try
            {
                var jsonResult = await _paymentService.CreatePaymentRequestAsync(invoiceId);
                return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("confirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirmation([FromForm] PayUWebhookRequest request)
        {
            // PayU sends confirmation via POST with form-data
            // We need to map it to our internal model
            try
            {
                var data = new PayUConfirmationData
                {
                    MerchantId = request.merchant_id,
                    ReferenceCode = request.reference_sale,
                    Amount = decimal.Parse(request.value, CultureInfo.InvariantCulture),
                    Currency = request.currency,
                    State = int.Parse(request.state_pol),
                    Signature = request.sign,
                    TransactionId = request.transaction_id,
                    ResponseMessage = request.response_message_pol
                };

                await _paymentService.ProcessPaymentConfirmationAsync(data);
                return Ok();
            }
            catch (Exception)
            {
                // Even if it fails, we usually return 200 to PayU to stop retries if we can't process it,
                // or return error if we want retries. PayU retries several times.
                return Ok(); 
            }
        }
    }

    public class PayUWebhookRequest
    {
        public string merchant_id { get; set; }
        public string state_pol { get; set; }
        public string reference_sale { get; set; }
        public string reference_pol { get; set; }
        public string sign { get; set; }
        public string value { get; set; }
        public string currency { get; set; }
        public string transaction_id { get; set; }
        public string response_message_pol { get; set; }
    }
}
