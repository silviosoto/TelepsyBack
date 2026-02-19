using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentRequestAsync(int invoiceId);
        Task<bool> ProcessPaymentConfirmationAsync(PayUConfirmationData confirmationData);
    }

    public class PayUConfirmationData
    {
        public string MerchantId { get; set; }
        public string ReferenceCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int State { get; set; } // 4 = Approved, 6 = Declined, 5 = Expired
        public string Signature { get; set; }
        public string TransactionId { get; set; }
        public string ResponseMessage { get; set; }
    }
}
