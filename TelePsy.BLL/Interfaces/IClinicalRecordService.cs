using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IClinicalRecordService
    {
        Task CreateRecordAsync(ClinicalRecord record);
        Task<ClinicalRecord> CreateOrUpdateRecordAsync(ClinicalRecord record);
        Task<IEnumerable<ClinicalRecord>> GetRecordsForPatientAsync(int patientId);
        Task<ClinicalRecord?> GetLatestRecordForPatientAsync(int patientId);
        Task<bool> HasHistoryForPatientAsync(int patientId);
    }
}
