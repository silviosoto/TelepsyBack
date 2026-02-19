using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Interfaces
{
    public interface IPsychologistTherapyService
    {
        Task<IEnumerable<PsychologistTherapyDto>> GetByPsychologistIdAsync(int psychologistId);
        Task SetRateAsync(int psychologistId, SetRateDto dto);
        Task RemoveTherapyAsync(int psychologistId, int therapyId);
    }
}
