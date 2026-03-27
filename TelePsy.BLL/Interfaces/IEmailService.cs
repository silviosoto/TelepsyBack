using System.Threading.Tasks;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendAppointmentNotificationAsync(Appointment appointment);
        Task SendWelcomeEmailAsync(User user, string firstName);
        Task SendPaymentConfirmationAsync(Appointment appointment);
        Task SendAppointmentChangeNotificationAsync(Appointment appointment, string reason, string role);
        Task SendPasswordResetEmailAsync(User user, string resetLink);
    }
}
