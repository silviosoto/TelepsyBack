using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetPatientAppointmentsByUserIdAsync(string userId);
        Task<IEnumerable<Appointment>> GetAppointmentsForPsychologistAsync(int psychologistId);
        Task CancelAppointmentAsync(int appointmentId);
        Task RescheduleAppointmentAsync(int appointmentId, DateTime newDate);
        Task<IEnumerable<WorkScheduleDto>> GetWorkScheduleAsync(int psychologistId);

        // Schedule Management
        Task SetWorkScheduleAsync(int psychologistId, List<WorkScheduleDto> schedules);

        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int psychologistId, DateTime date,
            int durationMinutes = 45);

        Task BlockSlotAsync(int psychologistId, BlockSlotDto dto);

        // Booking Flow
        Task<BookingResponseDto> InitiateBookingAsync(string userId, InitiateBookingDto dto);
        Task<CheckoutSummaryDto> GetCheckoutSummaryAsync(int appointmentId);
    }
}
