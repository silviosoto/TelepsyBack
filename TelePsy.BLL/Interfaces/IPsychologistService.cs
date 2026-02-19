using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IPsychologistService
    {
        Task<Psychologist?> GetPsychologistByIdAsync(int id);
        Task<Psychologist?> GetPsychologistByUserIdAsync(string userId);
        Task UpdateProfileAsync(int psychologistId, PsychologistProfileDto dto);
        Task<IEnumerable<Psychologist>> GetAllPsychologistsAsync();
        Task<IEnumerable<Psychologist>> GetVerifiedPsychologistsAsync();
        Task UploadCvAsync(int psychologistId, Stream fileStream, string fileName);
        Task UploadProfilePictureAsync(int psychologistId, Stream fileStream, string fileName);
    }
}
