using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IVideoService
    {
        Task<string> GenerateMeetingLinkAsync(Appointment appointment);
    }
}
