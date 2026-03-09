using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<Psychologist>> GetPendingPsychologistsAsync();
        Task ApprovePsychologistAsync(int psychologistId);
        Task RejectPsychologistAsync(int psychologistId, string reason);
        Task<decimal> GetCommissionRateAsync();
        Task UpdateCommissionRateAsync(decimal rate);
        Task<(IEnumerable<Patient> Patients, int TotalCount)> GetPatientsAsync(int page, int pageSize, string? searchTerm, DateTime? creationDate);
        Task<(IEnumerable<Psychologist> Psychologists, int TotalCount)> GetPsychologistsAsync(int page, int pageSize, string? searchTerm, bool? isVerified, DateTime? creationDate);
    }
}
