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

        public async Task SendWelcomeEmailAsync(User user, string firstName)
        {
            var message = new EmailMessage();
            message.From = _fromEmail;
            message.To.Add(user.Email);
            message.Subject = "¡Bienvenido a TelePsy! 💙";
            message.HtmlBody = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #4A90E2;'>¡Hola, {firstName}!</h2>
                    <p>Te damos la bienvenida a <strong>TelePsy</strong>, tu plataforma de confianza para el cuidado de la salud mental.</p>
                    <p>Estamos aquí para acompañarte en tu proceso y brindarte las mejores herramientas.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:3000' style='background-color: #4A90E2; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Empezar ahora</a>
                    </div>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='color: #777; font-size: 12px;'>Este es un mensaje automático, por favor no respondas a este correo.</p>
                </div>";

            await _resend.EmailSendAsync(message);
        }

        public async Task SendPaymentConfirmationAsync(Appointment appointment)
        {
            if (appointment?.Patient?.Person?.User == null)
            {
                throw new ArgumentException("Patient user information is missing.");
            }

            var patientEmail = appointment.Patient.Person.User.Email;
            var patientName = appointment.Patient.Person.FirstName;
            var psychologistName = $"{appointment.Psychologist.Person.FirstName} {appointment.Psychologist.Person.LastName}";
            var appointmentDate = appointment.ScheduledTime.ToString("dd/MM/yyyy");
            var appointmentTime = appointment.ScheduledTime.ToString("HH:mm");
            var videoLink = appointment.VideoLink;

            var message = new EmailMessage();
            message.From = _fromEmail;
            message.To.Add(patientEmail);
            message.Subject = "¡Tu cita en TelePsy está confirmada! ✅";
            message.HtmlBody = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #4A90E2;'>¡Hola, {patientName}!</h2>
                    <p>Tu pago ha sido procesado correctamente y tu cita ha sido confirmada.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p><strong>Profesional:</strong> Dr. {psychologistName}</p>
                    <p><strong>Fecha:</strong> {appointmentDate}</p>
                    <p><strong>Hora:</strong> {appointmentTime}</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <p>Podrás unirte a la videollamada usando el siguiente enlace:</p>
                        <a href='{videoLink}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Unirse a la Sesión (Zoom)</a>
                    </div>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='color: #777; font-size: 12px;'>Recuerda estar en un lugar tranquilo y con buena conexión a internet.</p>
                </div>";

            await _resend.EmailSendAsync(message);
        }

        public async Task SendAppointmentChangeNotificationAsync(Appointment appointment, string reason, string role)
        {
            var recipientEmail = "";
            var recipientName = "";
            var subject = "";
            var title = "";

            if (role == "Patient")
            {
                recipientEmail = appointment.Psychologist.Person.User.Email;
                recipientName = $"Dr. {appointment.Psychologist.Person.LastName}";
                subject = "Cambio en una de tus citas - TelePsy";
                title = "Actualización de Cita";
            }
            else
            {
                recipientEmail = appointment.Patient.Person.User.Email;
                recipientName = appointment.Patient.Person.FirstName;
                subject = "Tu cita en TelePsy ha sido actualizada";
                title = "Aviso de Cambio en tu Cita";
            }

            var appointmentDate = appointment.ScheduledTime.ToString("dd/MM/yyyy");
            var appointmentTime = appointment.ScheduledTime.ToString("HH:mm");

            var message = new EmailMessage();
            message.From = _fromEmail;
            message.To.Add(recipientEmail);
            message.Subject = subject;
            message.HtmlBody = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #e67e22;'>{title}</h2>
                    <p>Hola, {recipientName}, queremos informarte que se ha realizado un cambio en una cita programada.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p><strong>Motivo/Acción:</strong> {reason}</p>
                    <p><strong>Nueva Fecha/Estado:</strong> {appointmentDate} a las {appointmentTime}</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p>Puedes verificar los detalles actualizados en tu perfil.</p>
                    <p style='color: #777; font-size: 12px;'>Si tienes alguna duda, por favor contáctanos.</p>
                </div>";

            await _resend.EmailSendAsync(message);
        }
    }
}
