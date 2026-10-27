using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IPsychologyNoteService
    {
        Task CreateNoteAsync(PsychologyNote note);
        Task UpdateNoteAsync(PsychologyNote note);
        Task<IEnumerable<PsychologyNote>> GetNotesForPatientAsync(int patientId, int psychologistId);
        Task<IEnumerable<PsychologyNote>> GetNotesForAttentionRecordAsync(int serviceAttentionRecordId);
        Task<PsychologyNote?> GetNoteByAppointmentIdAsync(int appointmentId);
        Task<PsychologyNote?> GetByIdAsync(int id);
    }
}
