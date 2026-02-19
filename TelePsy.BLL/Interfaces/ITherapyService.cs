using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface ITherapyService
    {
        Task<IEnumerable<TherapyDto>> GetAllAsync();
        Task<TherapyDto> GetByIdAsync(int id);
        Task<Therapy> CreateAsync(CreateTherapyDto dto);
        Task UpdateAsync(int id, CreateTherapyDto dto);
        Task ToggleStatusAsync(int id);
    }
}
