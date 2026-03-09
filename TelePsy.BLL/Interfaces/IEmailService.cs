using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendAppointmentNotificationAsync(Appointment appointment);
    }
}
