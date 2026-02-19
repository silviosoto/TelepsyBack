using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;


namespace TelePsy.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            // Validation: Check availability, etc.
            
            // Set Rate based on Therapy
            if (appointment.TherapyId > 0)
            {
                var therapyConfig = await _unitOfWork.Repository<PsychologistTherapy>().GetFirstOrDefaultAsync(
                    pt => pt.PsychologistId == appointment.PsychologistId && pt.TherapyId == appointment.TherapyId && pt.IsActive
                );

                if (therapyConfig != null)
                {
                    appointment.Rate = therapyConfig.Rate;
                }
                else
                {
                    // Fallback or Error? 
                    // For now, let's assume if therapy is selected but not configured, it's invalid.
                    throw new Exception("Selected therapy is not available for this psychologist.");
                }
            }
            else
            {
                 // Legacy support or default rate? 
                 // If TherapyId is not provided, maybe use default SessionRate from Psychologist if available?
                 // But user insisted on Therapy Rate.
                 // Let's check Psychologist.SessionRate as fallback if no Therapy model strictly enforced yet.
                 var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(appointment.PsychologistId);
                 if (psychologist != null)
                 {
                     appointment.Rate = psychologist.SessionRate;
                 }
            }

            appointment.Status = AppointmentStatus.Pending;
            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.CompleteAsync();
            return appointment;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(int patientId)
        {
            return await _unitOfWork.Repository<Appointment>().GetAsync(a => a.PatientId == patientId,
                includeProperties: "Patient.Person,Psychologist.Person");
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForPsychologistAsync(int psychologistId)
        {
            return await _unitOfWork.Repository<Appointment>().GetAsync(a => a.PsychologistId == psychologistId,
                includeProperties: "Patient.Person,Psychologist.Person");
        }

        public async Task CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                _unitOfWork.Repository<Appointment>().Update(appointment);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task SetWorkScheduleAsync(int psychologistId, List<WorkScheduleDto> schedules)
        {
            // Remove existing schedules
            var existing = await _unitOfWork.Repository<WorkSchedule>()
                .GetAsync(w => w.PsychologistId == psychologistId);
            foreach (var schedule in existing)
            {
                _unitOfWork.Repository<WorkSchedule>().Delete(schedule);
            }

            // Add new schedules
            foreach (var dto in schedules)
            {
                var workSchedule = new WorkSchedule
                {
                    PsychologistId = psychologistId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                };
                await _unitOfWork.Repository<WorkSchedule>().AddAsync(workSchedule);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int psychologistId, DateTime date,
            int durationMinutes = 45)
        {
            var dayOfWeek = date.DayOfWeek;
            var schedules = await _unitOfWork.Repository<WorkSchedule>().GetAsync(w =>
                w.PsychologistId == psychologistId && w.DayOfWeek == dayOfWeek && w.IsActive
            );

            var slots = new List<AvailableSlotDto>();

            foreach (var schedule in schedules)
            {
                var currentTime = schedule.StartTime;
                while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= schedule.EndTime)
                {
                    var slotStart = date.Date.Add(currentTime);
                    var slotEnd = slotStart.AddMinutes(durationMinutes);

                    // Check if slot is blocked
                    var blockedSlots = await _unitOfWork.Repository<BlockedSlot>().GetAsync(b =>
                        b.PsychologistId == psychologistId &&
                        b.StartDateTime < slotEnd &&
                        b.EndDateTime > slotStart
                    );

                    // Check if slot has appointment
                    var appointments = await _unitOfWork.Repository<Appointment>().GetAsync(a =>
                        a.PsychologistId == psychologistId &&
                        a.ScheduledTime < slotEnd &&
                        a.ScheduledTime.AddMinutes(a.DurationMinutes) > slotStart &&
                        a.Status != AppointmentStatus.Cancelled
                    );

                    if (!blockedSlots.Any() && !appointments.Any())
                    {
                        slots.Add(new AvailableSlotDto
                        {
                            StartTime = slotStart,
                            EndTime = slotEnd
                        });
                    }

                    currentTime = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));
                }
            }

            return slots;
        }

        public async Task BlockSlotAsync(int psychologistId, BlockSlotDto dto)
        {
            var blockedSlot = new BlockedSlot
            {
                PsychologistId = psychologistId,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Reason = dto.Reason
            };

            await _unitOfWork.Repository<BlockedSlot>().AddAsync(blockedSlot);
            await _unitOfWork.CompleteAsync();
        }
    }
}
