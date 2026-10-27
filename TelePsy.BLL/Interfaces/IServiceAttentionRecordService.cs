using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Interfaces
{
    public interface IServiceAttentionRecordService
    {
        Task<ServiceAttentionRecordDto?> GetActiveRecordForServiceAsync(int patientId, int therapyId);
        Task<IEnumerable<ServiceAttentionRecordDto>> GetRecordsForPatientAsync(int patientId);
        Task<ServiceAttentionRecordDto?> GetByIdAsync(int id);
        Task<ServiceAttentionRecordDto> CreateRecordAsync(CreateServiceAttentionRecordDto dto, int psychologistId);
        Task<ServiceAttentionRecordDto> UpdateRecordAsync(int id, UpdateServiceAttentionRecordDto dto, int psychologistId);
    }
}
