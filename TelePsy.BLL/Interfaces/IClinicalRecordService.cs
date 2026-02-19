using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IClinicalRecordService
    {
        Task CreateRecordAsync(ClinicalRecord record);
        Task<IEnumerable<ClinicalRecord>> GetRecordsForPatientAsync(int patientId);
    }
}
