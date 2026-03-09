using Microsoft.Extensions.Configuration;
using Resend;
using System;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _fromEmail;

        public EmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
        }

        public async Task SendAppointmentNotificationAsync(Appointment appointment)
        {
            if (appointment?.Psychologist?.Person?.User == null)
            {
                throw new ArgumentException("Psychologist user information is missing.");
            }

            var psychologistEmail = appointment.Psychologist.Person.User.Email;
            var psychologistName = $"{appointment.Psychologist.Person.FirstName} {appointment.Psychologist.Person.LastName}";
            var patientName = $"{appointment.Patient.Person.FirstName} {appointment.Patient.Person.LastName}";
            var appointmentDate = appointment.ScheduledTime.ToString("dd/MM/yyyy");
            var appointmentTime = appointment.ScheduledTime.ToString("HH:mm");
            var therapyName = appointment.Therapy?.Name ?? "Sesión General";

            var message = new EmailMessage();
            message.From = _fromEmail;
            message.To.Add(psychologistEmail);
            message.Subject = "Nueva Cita Reservada - TelePsy";
            message.HtmlBody = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #4A90E2;'>¡Hola, Dr. {psychologistName}!</h2>
                    <p>Tienes una nueva cita confirmada en TelePsy.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p><strong>Paciente:</strong> {patientName}</p>
                    <p><strong>Tipo de Terapia:</strong> {therapyName}</p>
                    <p><strong>Fecha:</strong> {appointmentDate}</p>
                    <p><strong>Hora:</strong> {appointmentTime}</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p>Puedes ver los detalles en tu panel de control.</p>
                    <p style='color: #777; font-size: 12px;'>Este es un mensaje automático, por favor no respondas a este correo.</p>
                </div>";

            await _resend.EmailSendAsync(message);
        }
    }
}
