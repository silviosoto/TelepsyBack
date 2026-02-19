using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelePsy.BLL.Configuration;
using TelePsy.BLL.Interfaces;
using TelePsy.Domain.Entities;
using ZoomNet;

namespace TelePsy.BLL.Services
{
    public class ZoomVideoService : IVideoService
    {
        private readonly ZoomSettings _zoomSettings;
        private readonly ILogger<ZoomVideoService> _logger;

        public ZoomVideoService(
            IOptions<ZoomSettings> zoomSettings,
            ILogger<ZoomVideoService> logger)
        {
            _zoomSettings = zoomSettings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateMeetingLinkAsync(Appointment appointment)
        {
            try
            {
                _logger.LogInformation(
                    "Creating Zoom meeting for appointment {AppointmentId} scheduled at {ScheduledTime}",
                    appointment.Id,
                    appointment.ScheduledTime);

                // Create connection info for Server-to-Server OAuth
                var connectionInfo = OAuthConnectionInfo.ForServerToServer(
                    _zoomSettings.ClientId,
                    _zoomSettings.ClientSecret,
                    _zoomSettings.AccountId);

                // Create Zoom client
                using var zoomClient = new ZoomClient(connectionInfo);

                // Get patient and psychologist names
                var patientName = appointment.Patient?.Person?.FirstName ?? "Paciente";
                var psychologistName = appointment.Psychologist?.Person?.FirstName ?? "Psicólogo";

                // Create the meeting with default settings
                var meeting = await zoomClient.Meetings.CreateScheduledMeetingAsync(
                    userId: "me", // "me" refers to the account owner
                    topic: $"Sesión de Psicología - {patientName}",
                    agenda: $"Cita con {psychologistName}",
                    start: appointment.ScheduledTime,
                    duration: appointment.DurationMinutes,
                    timeZone: null, // Use account's default timezone
                    settings: null, // Use default meeting settings
                    cancellationToken: default);

                _logger.LogInformation(
                    "Successfully created Zoom meeting {MeetingId} for appointment {AppointmentId}. Join URL: {JoinUrl}",
                    meeting.Id,
                    appointment.Id,
                    meeting.JoinUrl);

                return meeting.JoinUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating Zoom meeting for appointment {AppointmentId}",
                    appointment.Id);

                throw new InvalidOperationException(
                    $"Failed to create Zoom meeting for appointment {appointment.Id}. Please try again later.",
                    ex);
            }
        }
    }
}
