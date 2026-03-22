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
            try
            {
                Console.WriteLine($"Webhook Received: State={request.state_pol}, Ref={request.reference_sale}, Value={request.value}, Sign={request.sign}");

                if (string.IsNullOrEmpty(request.state_pol))
                {
                    Console.WriteLine("state_pol is empty.");
                    return Ok();
                }

                int state = int.Parse(request.state_pol);
                
                // Allow replacing comma with dot for culture invariant parsing just in case
                string valueStr = request.value?.Replace(',', '.') ?? "0";
                decimal amount = decimal.Parse(valueStr, CultureInfo.InvariantCulture);

                var data = new PayUConfirmationData
                {
                    MerchantId = request.merchant_id,
                    ReferenceCode = request.reference_sale,
                    Amount = amount,
                    Currency = request.currency,
                    State = state,
                    Signature = request.sign,
                    TransactionId = request.transaction_id,
                    ResponseMessage = request.response_message_pol
                };

                bool processResult = await _paymentService.ProcessPaymentConfirmationAsync(data);
                Console.WriteLine($"Process Result: {processResult}");
                
                if (!processResult)
                {
                    return BadRequest(new { message = "Payment confirmation failed processing (e.g. invalid signature or missing record)." });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook processing error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500); 
            }
        }
    }

    public class PayUWebhookRequest
    {
        public string? merchant_id { get; set; }
        public string? state_pol { get; set; }
        public string? reference_sale { get; set; }
        public string? reference_pol { get; set; }
        public string? sign { get; set; }
        public string? value { get; set; }
        public string? currency { get; set; }
        public string? transaction_id { get; set; }
        public string? response_message_pol { get; set; }
    }
}
