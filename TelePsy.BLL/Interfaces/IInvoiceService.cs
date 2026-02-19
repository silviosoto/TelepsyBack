using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice> GeneratePatientInvoiceAsync(int paymentId);
        Task<Invoice> GeneratePsychologistPayoutAsync(int psychologistId, List<int> appointmentIds);
        Task<Invoice> GetInvoiceByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetUnpaidAppointmentsForPsychologistAsync(int psychologistId);
        Task UpdateGlobalCommissionAsync(decimal rate);
        Task<decimal> GetGlobalCommissionAsync();
    }
}
