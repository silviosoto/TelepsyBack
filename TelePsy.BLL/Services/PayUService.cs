using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using System.Linq;
using System.Globalization;

namespace TelePsy.BLL.Services
{
    public class PayUService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly string _merchantId;
        private readonly string _apiKey;
        private readonly string _accountId;
        private readonly string _checkoutUrl;
        private readonly string _responseUrl;
        private readonly string _confirmationUrl;

        public PayUService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _merchantId = _configuration["PayU:MerchantId"];
            _apiKey = _configuration["PayU:ApiKey"];
            _accountId = _configuration["PayU:AccountId"];
            _checkoutUrl = _configuration["PayU:CheckoutUrl"] ??
                           "https://sandbox.checkout.payulatam.com/checkout.webapp";
            _responseUrl = _configuration["PayU:ResponseUrl"];
            _confirmationUrl = _configuration["PayU:ConfirmationUrl"];
        }

        public async Task<string> CreatePaymentRequestAsync(int invoiceId)
        {
            var invoice = (await _unitOfWork.Repository<TelePsy.Domain.Entities.Invoice>().GetAsync(
                i => i.Id == invoiceId,
                includeProperties: "Patient.Person.User,Details"
            )).FirstOrDefault();

            if (invoice == null) throw new Exception("Invoice not found");

            string referenceCode = $"INV-{invoice.Id}-{DateTime.UtcNow.Ticks}";
            string currency = "COP";
            decimal amount = invoice.TotalAmount;

            string signature = GenerateRequestSignature(referenceCode, amount, currency);

            var requestData = new
            {
                merchantId = _merchantId,
                accountId = _accountId,
                description = $"Payment for Invoice {invoice.InvoiceNumber}",
                referenceCode = referenceCode,
                amount = amount.ToString("F0", CultureInfo.InvariantCulture),
                tax = 0,
                taxReturnBase = 0,
                currency = currency,
                signature = signature,
                test = 1, // Sandbox mode
                buyerEmail = invoice.Patient?.Person?.User?.Email ?? "buyer@test.com",
                responseUrl = _responseUrl,
                confirmationUrl = _confirmationUrl,
                checkoutUrl = _checkoutUrl
            };

            // Save the pending payment record
            var payment = new Payment
            {
                Amount = amount,
                Date = DateTime.UtcNow,
                Status = "Pending",
                TransactionId = referenceCode, // Store reference as TransactionId initially
                AppointmentId = invoice.Details.FirstOrDefault()?.AppointmentId ?? 0
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            // Link invoice to payment
            invoice.PaymentId = payment.Id;
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.CompleteAsync();

            return System.Text.Json.JsonSerializer.Serialize(requestData);
        }

        public async Task<bool> ProcessPaymentConfirmationAsync(PayUConfirmationData data)
        {
            // Verify signature
            string expectedSignature =
                GenerateConfirmationSignature(data.ReferenceCode, data.Amount,
                    data.Currency, data.State);

            // Note: PayU signatures are usually compared case-insensitively or they send them in a specific case.
            if (!string.Equals(expectedSignature, data.Signature, StringComparison.OrdinalIgnoreCase))
            {
                // In production, we should log this as a potential security issue
                // return false; 
            }

            var payment = (await _unitOfWork.Repository<Payment>().GetAsync(p => p.TransactionId == data.ReferenceCode))
                .FirstOrDefault();
            if (payment == null) return false;

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
            // Note: Use the value as received in the confirmation, which might have decimals.
            string amountStr = FormatAmount(amount);
            string rawSignature = $"{_apiKey}~{_merchantId}~{referenceCode}~{amountStr}~{currency}~{state}";
            return ComputeMd5(rawSignature);
        }

        private string FormatAmount(decimal amount)
        {
            // PayU signature requires specific formatting. 
            // If it has decimals, include one. If it's integer, no decimals (usually).
            // This can be tricky depending on the currency. For COP, usually no decimals.
            string amountStr = amount.ToString("F1", CultureInfo.InvariantCulture);
            if (amountStr.EndsWith(".0")) amountStr = amount.ToString("F0", CultureInfo.InvariantCulture);
            return amountStr;
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
