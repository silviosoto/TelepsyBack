using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEmailService _emailService;
        private readonly IVideoService _videoService;

        public AppointmentService(IUnitOfWork unitOfWork, IEmailService emailService, IVideoService videoService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _videoService = videoService;
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

                if (therapyConfig is not null)
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
                 if (psychologist is not null)
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
                includeProperties: "Patient.Person,Psychologist.Person,Therapy,SessionPackage");
        }

        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsByUserIdAsync(string userId)
        {
            var patient = (await _unitOfWork.Repository<Patient>().GetAsync(p => p.Person.UserId == userId)).FirstOrDefault();
            if (patient == null) return new List<Appointment>();

            return await GetAppointmentsForPatientAsync(patient.Id);
        }

        public async Task<IEnumerable<SessionPackage>> GetActivePackagesForPatientAsync(string userId)
        {
            var patient = (await _unitOfWork.Repository<Patient>().GetAsync(p => p.Person.UserId == userId)).FirstOrDefault();
            if (patient == null) return new List<SessionPackage>();

            return await _unitOfWork.Repository<SessionPackage>().GetAsync(
                sp => sp.PatientId == patient.Id && sp.IsActive && sp.UsedSessions < sp.TotalSessions,
                includeProperties: "Psychologist.Person,Therapy");
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForPsychologistAsync(int psychologistId)
        {
            return await _unitOfWork.Repository<Appointment>().GetAsync(a => a.PsychologistId == psychologistId,
                includeProperties: "Patient.Person,Psychologist.Person,SessionPackage");
        }

        public async Task CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetFirstOrDefaultAsync(
                a => a.Id == appointmentId,
                includeProperties: "Patient.Person.User,Psychologist.Person.User"
            );

            if (appointment is not null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                _unitOfWork.Repository<Appointment>().Update(appointment);
                await _unitOfWork.CompleteAsync();

                try
                {
                    // Generic notification, logic deciding recipient is in EmailService
                    await _emailService.SendAppointmentChangeNotificationAsync(appointment, "Cita Cancelada", "System");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending cancellation email: {ex.Message}");
                }
            }
        }

        public async Task RescheduleAppointmentAsync(int appointmentId, DateTime newDate)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetFirstOrDefaultAsync(
                a => a.Id == appointmentId,
                includeProperties: "Patient.Person.User,Psychologist.Person.User"
            );

            if (appointment == null) throw new Exception("Appointment not found");

            // Validation: Max 1 month from original date
            var limitDate = appointment.ScheduledTime.AddMonths(1);
            if (newDate > limitDate)
            {
                throw new Exception("La nueva fecha no puede ser superior a un mes de la fecha original.");
            }

            if (newDate < DateTime.UtcNow)
            {
                throw new Exception("La nueva fecha debe ser en el futuro.");
            }

            appointment.ScheduledTime = newDate;
            
            // If the appointment was already confirmed (paid), keep it confirmed.
            // Only move to Pending if it's a new or previously pending appointment.
            if (appointment.Status != AppointmentStatus.Confirmed && appointment.Status != AppointmentStatus.Completed)
            {
                appointment.Status = AppointmentStatus.Pending;
            }
            
            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.CompleteAsync();

            try
            {
                await _emailService.SendAppointmentChangeNotificationAsync(appointment, "Cita Reagendada", "System");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending reschedule email: {ex.Message}");
            }
        }

        public async Task<IEnumerable<WorkScheduleDto>> GetWorkScheduleAsync(int psychologistId)
        {
            var schedule = await _unitOfWork.Repository<WorkSchedule>().GetAsync(w => w.PsychologistId == psychologistId);
            return schedule.Select(s => new WorkScheduleDto
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            });
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
                    // Create date-time from parts and convert to UTC based on server's timezone
                    // This ensures consistency with the rest of the UTC-based system
                    var slotStart = date.Date.Add(currentTime).ToUniversalTime();
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

        public async Task<BookingResponseDto> InitiateBookingAsync(string userId, InitiateBookingDto dto)
        {
            // 1. Get PatientId from UserId
            var person = (await _unitOfWork.Repository<Person>().GetAsync(p => p.UserId == userId)).FirstOrDefault();
            if (person == null) throw new Exception("Person profile not found for user.");

            var patient = (await _unitOfWork.Repository<Patient>().GetAsync(p => p.PersonId == person.Id)).FirstOrDefault();
            if (patient == null) throw new Exception("Patient profile not found.");

            // 2. Validate availability (simple check for now)
            var isBusy = (await _unitOfWork.Repository<Appointment>().GetAsync(a =>
                a.PsychologistId == dto.PsychologistId &&
                a.ScheduledTime == dto.ScheduledTime &&
                a.Status != AppointmentStatus.Cancelled)).Any();

            if (isBusy) throw new Exception("The selected time slot is no longer available.");

            // 3. Prepare Appointment in Pending status
            var appointment = new Appointment
            {
                PatientId = patient.Id,
                PsychologistId = dto.PsychologistId,
                TherapyId = dto.TherapyId,
                ScheduledTime = dto.ScheduledTime,
                DurationMinutes = 45, // Default
                Status = AppointmentStatus.Pending,
                VideoLink = string.Empty // Will be generated after payment
            };

            // Calculate rate
            var therapyConfig = await _unitOfWork.Repository<PsychologistTherapy>().GetFirstOrDefaultAsync(
                pt => pt.PsychologistId == dto.PsychologistId && pt.TherapyId == dto.TherapyId && pt.IsActive
            );

            decimal baseRate = 0;
            if (therapyConfig != null)
            {
                baseRate = therapyConfig.Rate;
            }
            else
            {
                var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(dto.PsychologistId);
                baseRate = psychologist?.SessionRate ?? 0;
            }

            appointment.Rate = baseRate;

            int packageSessions = dto.PackageSessions ?? 1;

            // Check if patient already has an active package with available sessions
            var activePackage = (await _unitOfWork.Repository<SessionPackage>().GetAsync(sp =>
                sp.PatientId == patient.Id &&
                sp.PsychologistId == dto.PsychologistId &&
                sp.TherapyId == dto.TherapyId &&
                sp.IsActive &&
                sp.UsedSessions < sp.TotalSessions
            )).FirstOrDefault();

            if (activePackage != null && dto.PackageSessions == null) // If they have an active package and didn't explicitly request to buy a new one
            {
                appointment.Status = AppointmentStatus.Confirmed; // Already paid via package!
                appointment.SessionPackageId = activePackage.Id;
                
                activePackage.UsedSessions++;
                _unitOfWork.Repository<SessionPackage>().Update(activePackage);

                // Generate Zoom link
                try
                {
                    // Mock object to pass to video service or fetch full
                    appointment.Patient = patient;
                    var psychologist = await _unitOfWork.Repository<Psychologist>().GetFirstOrDefaultAsync(p => p.Id == dto.PsychologistId, includeProperties: "Person.User");
                    appointment.Psychologist = psychologist;
                    appointment.VideoLink = await _videoService.GenerateMeetingLinkAsync(appointment);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating Zoom link: {ex.Message}");
                }

                await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
                await _unitOfWork.CompleteAsync();

                return new BookingResponseDto
                {
                    AppointmentId = appointment.Id,
                    Message = "Booking confirmed using active package"
                };
            }

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.CompleteAsync(); // Save to get Appointment.Id

            // If a new package is being purchased
            SessionPackage? newPackage = null;
            decimal totalAmount = baseRate;

            if (packageSessions > 1)
            {
                // Get discount
                string configKey = packageSessions switch
                {
                    4 => "PackageDiscount4Sessions",
                    8 => "PackageDiscount8Sessions",
                    12 => "PackageDiscount12Sessions",
                    _ => ""
                };

                decimal discountPercentage = 0;
                if (!string.IsNullOrEmpty(configKey))
                {
                    var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(c => c.Key == configKey)).FirstOrDefault();
                    if (config != null && decimal.TryParse(config.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedDiscount))
                    {
                        discountPercentage = parsedDiscount;
                    }
                }

                decimal originalTotal = baseRate * packageSessions;
                totalAmount = originalTotal * (1 - discountPercentage);

                newPackage = new SessionPackage
                {
                    PatientId = patient.Id,
                    PsychologistId = dto.PsychologistId,
                    TherapyId = dto.TherapyId,
                    OriginalTotalAmount = originalTotal,
                    DiscountPercentage = discountPercentage,
                    FinalAmount = totalAmount,
                    TotalSessions = packageSessions,
                    UsedSessions = 1, // This first appointment uses 1 session immediately
                    IsActive = false, // Will be activated upon payment
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<SessionPackage>().AddAsync(newPackage);
                await _unitOfWork.CompleteAsync();

                // Link the appointment to the pending package
                appointment.SessionPackageId = newPackage.Id;
                _unitOfWork.Repository<Appointment>().Update(appointment);
            }

            // 4. Create Invoice for the appointment (or package)
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{appointment.Id}-{Guid.NewGuid().ToString().Substring(0, 4)}",
                IssueDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Type = InvoiceType.ClientPurchase,
                Status = InvoiceStatus.Issued,
                PatientId = patient.Id
            };

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _unitOfWork.CompleteAsync(); // Save to get Invoice.Id

            // 5. Create InvoiceDetail to link them
            var colombiaTime = appointment.ScheduledTime.AddHours(-5);
            string description = packageSessions > 1 ? $"Paquete de {packageSessions} sesiones" : $"Sesión de Terapia - {colombiaTime:dd/MM/yyyy HH:mm}";
            var detail = new InvoiceDetail
            {
                InvoiceId = invoice.Id,
                AppointmentId = appointment.Id,
                Description = description,
                UnitPrice = totalAmount,
                Total = totalAmount,
                CommissionAmount = totalAmount * 0.30m // Assuming 30% commission
            };

            await _unitOfWork.Repository<InvoiceDetail>().AddAsync(detail);
            await _unitOfWork.CompleteAsync();
            
            return new BookingResponseDto
            {
                AppointmentId = appointment.Id,
                Message = "Booking initiated successfully"
            };
        }

        public async Task<CheckoutSummaryDto> GetCheckoutSummaryAsync(int appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetFirstOrDefaultAsync(
                a => a.Id == appointmentId,
                includeProperties: "Psychologist.Person,Therapy"
            );

            if (appointment == null) throw new Exception("Appointment not found");

            // Find the associated invoice
            // Note: Currently we don't have a direct link from Appointment to Invoice in the entity,
            // but we can find the invoice where the PatientId matches and it's a ClientPurchase for this amount?
            // Better: We should probably add an InvoiceId to Appointment or link them via InvoiceDetail.
            
            var invoiceDetail = (await _unitOfWork.Repository<InvoiceDetail>().GetAsync(d => d.AppointmentId == appointmentId)).FirstOrDefault();
            
            int invoiceId = 0;
            decimal checkoutAmount = appointment.Rate;

            if (invoiceDetail != null)
            {
                invoiceId = invoiceDetail.InvoiceId;
                checkoutAmount = invoiceDetail.Total;
            }
            else
            {
                // Fallback: search for the latest invoice for this patient created around now
                var invoice = (await _unitOfWork.Repository<Invoice>().GetAsync(
                    i => i.PatientId == appointment.PatientId && i.Type == InvoiceType.ClientPurchase)).OrderByDescending(i => i.Id).FirstOrDefault();
                invoiceId = invoice?.Id ?? 0;
            }

            return new CheckoutSummaryDto
            {
                AppointmentId = appointment.Id,
                InvoiceId = invoiceId,
                PsychologistName = appointment.Psychologist?.Person != null ? $"{appointment.Psychologist.Person.FirstName} {appointment.Psychologist.Person.LastName}" : "Unknown",
                TherapyName = appointment.Therapy?.Name ?? "General Session",
                ScheduledTime = appointment.ScheduledTime,
                Amount = checkoutAmount
            };
        }

        public async Task<string> JoinAppointmentAsync(int appointmentId, string userId, string role)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
            if (appointment == null) throw new Exception("Appointment not found");

            if (role == "Patient")
            {
                appointment.PatientJoinedAt = DateTime.UtcNow;
            }
            else if (role == "Psychologist")
            {
                appointment.PsychologistJoinedAt = DateTime.UtcNow;
            }

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.CompleteAsync();

            return appointment.VideoLink;
        }
    }
}
