using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IMatchingService
    {
        Task<IEnumerable<Psychologist>> GetMatchesForPatientAsync(int patientId);
    }
}
