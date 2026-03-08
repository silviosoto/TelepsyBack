using System.Threading.Tasks;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Interfaces
{
    public interface IPatientService
    {
        Task<PatientProfileDto> GetProfileAsync(string userId);
        Task UpdateProfileAsync(string userId, PatientProfileDto dto);
    }
}
