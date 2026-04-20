using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using System.Globalization;

namespace TelePsy.BLL.Services
{
    public class PayUService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IVideoService _videoService;
        private readonly string _merchantId;
        private readonly string _apiKey;
        private readonly string _accountId;
        private readonly string _checkoutUrl;
        private readonly string _responseUrl;
        private readonly string _confirmationUrl;
        private readonly bool _testMode;

        // public PayUService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService, IVideoService videoService)
        // {
        //     _unitOfWork = unitOfWork;
        //     _emailService = emailService;
        //     _videoService = videoService;
        //     _merchantId = "1026554";
        //     _apiKey = "cuM8hUU8eooHoNNQKbcZFajZii";
        //     _accountId = "1035772";
        //     _testMode = false;
        //     _checkoutUrl = "https://checkout.payulatam.com/ppp-web-gateway-payu/";
        //     _responseUrl = "http://localhost:3000/payment/response";
        //     _confirmationUrl = "https://1e3d-191-95-131-132.ngrok-free.app/api/payment/confirmation";
        // }

        public PayUService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService, IVideoService videoService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _videoService = videoService;
            _merchantId = configuration["PayU:MerchantId"] ?? string.Empty;
            _apiKey = configuration["PayU:ApiKey"] ?? string.Empty;
            _accountId = configuration["PayU:AccountId"] ?? string.Empty;
            _testMode = configuration.GetValue("PayU:TestMode", true);
            _checkoutUrl = configuration["PayU:CheckoutUrl"] ??
                           (_testMode ? "https://sandbox.checkout.payulatam.com/ppp-web-gateway-payu/" : "https://checkout.payulatam.com/ppp-web-gateway-payu/");
            _responseUrl = configuration["PayU:ResponseUrl"] ?? string.Empty;
            _confirmationUrl = configuration["PayU:ConfirmationUrl"] ?? string.Empty;
        }

        public async Task<string> CreatePaymentRequestAsync(int invoiceId)
        {
            var invoice = (await _unitOfWork.Repository<Invoice>().GetAsync(
                i => i.Id == invoiceId,
                includeProperties: "Patient.Person.User,Details"
            )).FirstOrDefault();

            if (invoice == null) throw new Exception("Invoice not found");

            var appointmentId = invoice.Details.FirstOrDefault()?.AppointmentId ?? 0;
            if (appointmentId == 0) throw new Exception("No appointment associated with this invoice.");

            string referenceCode = $"INV-{invoice.Id}-{DateTime.UtcNow.Ticks % 1000000}"; // Shorter but unique enough
            string currency = "COP";
            decimal amount = invoice.TotalAmount;

            string signature = GenerateRequestSignature(referenceCode, amount, currency);

            var requestData = new
            {
                merchantId = _merchantId,
                accountId = _accountId,
                description = $"Payment for Invoice {invoice.InvoiceNumber}",
                referenceCode,
                amount = FormatAmount(amount),
                tax = 0,
                taxReturnBase = 0,
                signature,
                test = _testMode ? 1 : 0,
                buyerEmail = invoice.Patient.Person.User.Email ?? "buyer@test.com",
                currency = "COP",
                responseUrl = _responseUrl,
                confirmationUrl = _confirmationUrl,
                checkoutUrl = _checkoutUrl
            };

            // Check if a payment already exists for this appointment
            var existingPayment = (await _unitOfWork.Repository<Payment>().GetAsync(p => p.AppointmentId == appointmentId)).FirstOrDefault();

            Payment paymentToSave;
            if (existingPayment != null && existingPayment.Status == "Pending")
            {
                // Reuse existing payment
                existingPayment.Amount = amount;
                existingPayment.TransactionId = referenceCode;
                existingPayment.Date = DateTime.UtcNow;
                existingPayment.PatientInvoiceId = invoice.Id;
                _unitOfWork.Repository<Payment>().Update(existingPayment);
                paymentToSave = existingPayment;
            }
            else
            {
                // Create a new pending payment record
                paymentToSave = new Payment
                {
                    Amount = amount,
                    Date = DateTime.UtcNow,
                    Status = "Pending",
                    TransactionId = referenceCode,
                    AppointmentId = appointmentId,
                    PatientInvoiceId = invoice.Id
                };

                await _unitOfWork.Repository<Payment>().AddAsync(paymentToSave);
            }

            await _unitOfWork.CompleteAsync();

            // Link invoice to payment after saving the payment to ensure we have a valid ID
            if (invoice.PaymentId != paymentToSave.Id)
            {
                invoice.PaymentId = paymentToSave.Id;
                _unitOfWork.Repository<Invoice>().Update(invoice);
                await _unitOfWork.CompleteAsync();
            }

            return System.Text.Json.JsonSerializer.Serialize(requestData);
        }

        public async Task<bool> ProcessPaymentConfirmationAsync(PayUConfirmationData data)
        {
            // Verify signature
            string expectedSignature =
                GenerateConfirmationSignature(data.ReferenceCode, data.Amount,
                    data.Currency, data.State);

            // Note: PayU signatures are usually compared case-insensitively.
            if (!string.Equals(expectedSignature, data.Signature, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Invalid Signature. Expected: {expectedSignature}, Received: {data.Signature}");
                return false;
            }

            var payment = (await _unitOfWork.Repository<Payment>().GetAsync(p => p.TransactionId == data.ReferenceCode))
                .FirstOrDefault();

            if (payment == null)
            {
                Console.WriteLine($"Payment with reference {data.ReferenceCode} not found.");
                return false;
            }

            // IDEMPOTENCY: If payment is already completed or failed, don't process again
            if (payment.Status == "Completed" || payment.Status == "Failed")
            {
                Console.WriteLine($"Payment {data.ReferenceCode} already processed with status: {payment.Status}. Skipping.");
                return true; // Return true because we already did the work, PayU should stop retrying
            }

            if (data.State == 4) // Approved
            {
                payment.Status = "Completed";
                payment.TransactionId = data.TransactionId; // Update with PayU's transaction ID

                var invoice = (await _unitOfWork.Repository<Invoice>().GetAsync(i => i.PaymentId == payment.Id))
                    .FirstOrDefault();
                if (invoice != null)
                {
                    invoice.Status = InvoiceStatus.Paid;
                    _unitOfWork.Repository<Invoice>().Update(invoice);
                }

                var appointment = (await _unitOfWork.Repository<Appointment>().GetAsync(
                    a => a.Id == payment.AppointmentId,
                    includeProperties: "Patient.Person,Psychologist.Person.User,Therapy,SessionPackage"
                )).FirstOrDefault();

                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Confirmed;

                    if (appointment.SessionPackage != null)
                    {
                        appointment.SessionPackage.IsActive = true;
                        appointment.SessionPackage.PaymentId = payment.Id;
                        _unitOfWork.Repository<SessionPackage>().Update(appointment.SessionPackage);
                    }

                    // Generate Zoom link if it's an online session
                    try
                    {
                        if (string.IsNullOrEmpty(appointment.VideoLink))
                        {
                            string zoomLink = await _videoService.GenerateMeetingLinkAsync(appointment);
                            appointment.VideoLink = zoomLink;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error generating Zoom link: {ex.Message}");
                    }

                    _unitOfWork.Repository<Appointment>().Update(appointment);

                    // Send email notification to psychologist
                    try
                    {
                        var fullAppointment = (await _unitOfWork.Repository<Appointment>().GetAsync(
                            a => a.Id == appointment.Id,
                            includeProperties: "Patient.Person,Psychologist.Person.User,Therapy"
                        )).FirstOrDefault();

                        if (fullAppointment != null)
                        {
                            await _emailService.SendAppointmentNotificationAsync(fullAppointment);
                            await _emailService.SendPaymentConfirmationAsync(fullAppointment);
                        }
                    }
                    catch (Exception ex)
                    {
                        // In production, log error but don't fail payment confirmation
                        Console.WriteLine($"Error sending email notification: {ex.Message}");
                    }
                }
            }
            else if (data.State == 6) // Declined
            {
                payment.Status = "Failed";
            }
            else if (data.State == 5) // Expired
            {
                payment.Status = "Expired";
            }

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private string GenerateRequestSignature(string referenceCode, decimal amount, string currency)
        {
            // Request Signature: ApiKey~merchantId~referenceCode~amount~currency
            string amountStr = FormatAmount(amount);
            string rawSignature = $"{_apiKey}~{_merchantId}~{referenceCode}~{amountStr}~{currency}";
            return ComputeMd5(rawSignature);
        }

        private string GenerateConfirmationSignature(string referenceCode, decimal amount, string currency, int state)
        {
            // Confirmation Signature: ApiKey~merchant_id~reference_sale~value~currency~state_pol
            // Note: Use exactly one decimal without dropping it even if it's .0 as per PayU specifications.
            string amountStr = amount.ToString("F1", CultureInfo.InvariantCulture);
            string rawSignature = $"{_apiKey}~{_merchantId}~{referenceCode}~{amountStr}~{currency}~{state}";
            return ComputeMd5(rawSignature);
        }

        private string FormatAmount(decimal amount)
        {
            // PayU signature requires specific formatting. 
            // For COP, it is recommended to use no decimals and the same string 
            // must be used in the 'amount' field and in the signature.
            return Math.Round(amount).ToString("F0", CultureInfo.InvariantCulture);
        }

        private string ComputeMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
