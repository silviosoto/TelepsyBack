using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IPsychologyNoteService
    {
        Task CreateNoteAsync(PsychologyNote note);
        Task<IEnumerable<PsychologyNote>> GetNotesForPatientAsync(int patientId, int psychologistId);
    }
}
