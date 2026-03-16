using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using TelePsy.Domain.DTOs;

namespace TelePsy.BLL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IInvoiceService _invoiceService;

        public AdminService(IUnitOfWork unitOfWork, IAuditService auditService, IInvoiceService invoiceService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _invoiceService = invoiceService;
        }

        public async Task<IEnumerable<Psychologist>> GetPendingPsychologistsAsync()
        {
            return await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => !p.IsVerified && p.IsActive,
                includeProperties: "Person"
            );
        }

        public async Task ApprovePsychologistAsync(int psychologistId)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            psychologist.IsVerified = true;
            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Approve", "Psychologist", 
                psychologistId.ToString(), "Psychologist approved");
        }

        public async Task RejectPsychologistAsync(int psychologistId, string reason)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            psychologist.IsActive = false;
            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Reject", "Psychologist", 
                psychologistId.ToString(), $"Psychologist rejected: {reason}");
        }

        public async Task<decimal> GetCommissionRateAsync()
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(
                c => c.Key == "CommissionRate"
            )).FirstOrDefault();

            if (config != null && decimal.TryParse(config.Value, out var rate))
            {
                return rate;
            }

            return 0.30m; // Default
        }

        public async Task UpdateCommissionRateAsync(decimal rate)
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(
                c => c.Key == "CommissionRate"
            )).FirstOrDefault();

            if (config == null)
            {
                config = new GlobalConfiguration
                {
                    Key = "CommissionRate",
                    Value = rate.ToString()
                };
                await _unitOfWork.Repository<GlobalConfiguration>().AddAsync(config);
            }
            else
            {
                config.Value = rate.ToString();
                config.LastUpdated = DateTime.UtcNow;
                _unitOfWork.Repository<GlobalConfiguration>().Update(config);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Update", "GlobalConfiguration", 
                "CommissionRate", $"Commission rate updated to {rate}");
        }

        public async Task<(IEnumerable<Patient> Patients, int TotalCount)> GetPatientsAsync(int page, int pageSize, string? searchTerm, DateTime? creationDate)
        {
            var query = await _unitOfWork.Repository<Patient>().GetAsync(includeProperties: "Person,Person.User");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Person.FirstName.ToLower().Contains(lowerSearch) ||
                    p.Person.LastName.ToLower().Contains(lowerSearch) ||
                    (p.Person.User != null && p.Person.User.Email != null && p.Person.User.Email.ToLower().Contains(lowerSearch))
                );
            }

            // Filtering by a specific date
            if (creationDate.HasValue)
            {
                query = query.Where(p => p.Person.User != null && p.Person.User.CreatedAt.Date == creationDate.Value.Date);
            }

            int totalCount = query.Count();

            // Pagination
            var pagedData = query
                .OrderByDescending(p => p.Person.User != null ? p.Person.User.CreatedAt : DateTime.MinValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        public async Task<(IEnumerable<Psychologist> Psychologists, int TotalCount)> GetPsychologistsAsync(int page, int pageSize, string? searchTerm, bool? isVerified, DateTime? creationDate)
        {
            var query = await _unitOfWork.Repository<Psychologist>().GetAsync(includeProperties: "Person,Person.User");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Person.FirstName.ToLower().Contains(lowerSearch) ||
                    p.Person.LastName.ToLower().Contains(lowerSearch) ||
                    (p.Person.User != null && p.Person.User.Email != null && p.Person.User.Email.ToLower().Contains(lowerSearch))
                );
            }

            if (isVerified.HasValue)
            {
                query = query.Where(p => p.IsVerified == isVerified.Value);
            }

            // Filtering by a specific date
            if (creationDate.HasValue)
            {
                query = query.Where(p => p.Person.User != null && p.Person.User.CreatedAt.Date == creationDate.Value.Date);
            }

            int totalCount = query.Count();

            var pagedData = query
                .OrderByDescending(p => p.Person.User != null ? p.Person.User.CreatedAt : DateTime.MinValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        public async Task<Psychologist?> GetPsychologistDetailsAsync(int id)
        {
            var psychologist = (await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => p.Id == id,
                includeProperties: "Person,Person.User,Specialties,Specialties.Specialty,Therapies,Therapies.Therapy"
            )).FirstOrDefault();

            return psychologist;
        }

        public async Task<(IEnumerable<Appointment> Appointments, int TotalCount)> GetPsychologistAppointmentsAsync(int psychologistId, int page, int pageSize, string? searchTerm, DateTime? startDate, DateTime? endDate)
        {
            var query = await _unitOfWork.Repository<Appointment>().GetAsync(
                a => a.PsychologistId == psychologistId,
                includeProperties: "Patient,Patient.Person,Therapy"
            );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(a => 
                    (a.Patient?.Person?.FirstName?.ToLower() ?? "").Contains(lowerSearch) ||
                    (a.Patient?.Person?.LastName?.ToLower() ?? "").Contains(lowerSearch)
                );
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.ScheduledTime.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.ScheduledTime.Date <= endDate.Value.Date);
            }

            int totalCount = query.Count();

            var pagedData = query
                .OrderByDescending(a => a.ScheduledTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPsychologistPaymentsAsync(int psychologistId, int page = 1, int pageSize = 10, string? searchTerm = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = await _unitOfWork.Repository<Payment>().GetAsync(
                p => p.Appointment.PsychologistId == psychologistId,
                includeProperties: "Appointment,Appointment.Patient,Appointment.Patient.Person"
            );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(p => 
                    (p.Appointment.Patient.Person.FirstName.ToLower() + " " + p.Appointment.Patient.Person.LastName.ToLower()).Contains(lowerSearch) ||
                    (p.TransactionId != null && p.TransactionId.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(p => p.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.Date.Date <= endDate.Value.Date);
            }

            int totalCount = query.Count();

            var pagedData = query
                .OrderByDescending(p => p.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        public async Task<IEnumerable<PaymentManagementDto>> GetPaymentManagementAsync(int? psychologistId = null, int? patientId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var invoices = await _unitOfWork.Repository<Invoice>().GetAsync(
                i => i.Type == InvoiceType.ClientPurchase && i.Status == InvoiceStatus.Paid,
                includeProperties: "Patient.Person,Details,Details.Appointment,Details.Appointment.Psychologist.Person,Details.Appointment.Therapy"
            );

            if (patientId.HasValue)
            {
                invoices = invoices.Where(i => i.PatientId == patientId.Value);
            }

            if (startDate.HasValue)
            {
                invoices = invoices.Where(i => i.IssueDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                invoices = invoices.Where(i => i.IssueDate.Date <= endDate.Value.Date);
            }

            var commissionRate = await _invoiceService.GetGlobalCommissionAsync();
            var result = new List<PaymentManagementDto>();

            foreach (var invoice in invoices)
            {
                foreach (var detail in invoice.Details)
                {
                    if (detail.Appointment == null) continue;

                    var appt = detail.Appointment;
                    
                    if (psychologistId.HasValue && appt.PsychologistId != psychologistId.Value)
                    {
                        continue;
                    }

                    var psychologist = appt.Psychologist;
                    var therapistName = psychologist?.Person != null 
                        ? $"{psychologist.Person.FirstName} {psychologist.Person.LastName}" 
                        : "Unknown";

                    decimal unitPrice = detail.UnitPrice;
                    decimal commission = unitPrice * commissionRate;
                    decimal psychoShare = unitPrice - commission;

                    result.Add(new PaymentManagementDto
                    {
                        InvoiceId = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        Date = invoice.IssueDate,
                        PatientId = invoice.PatientId ?? 0,
                        PatientName = invoice.Patient?.Person != null 
                            ? $"{invoice.Patient.Person.FirstName} {invoice.Patient.Person.LastName}" 
                            : "N/A",
                        PsychologistId = appt.PsychologistId,
                        PsychologistName = therapistName,
                        ServiceName = appt.Therapy?.Name ?? "General Session",
                        TotalAmount = unitPrice,
                        PsychologistShare = psychoShare,
                        PlatformCommission = commission,
                        IsPaidToPsychologist = appt.PsychologistInvoiceId != null,
                        PatientAttended = appt.PatientJoinedAt.HasValue,
                        PsychologistAttended = appt.PsychologistJoinedAt.HasValue,
                        AppointmentId = appt.Id
                    });
                }
            }

            return result.OrderByDescending(r => r.Date);
        }

        public async Task ProcessPsychologistPayoutAsync(PsychologistPayoutRequestDto request)
        {
            await _invoiceService.GeneratePsychologistPayoutAsync(request.PsychologistId, request.AppointmentIds);
            
            await _auditService.LogAsync("Admin", "Payout", "Invoice", 
                request.PsychologistId.ToString(), $"Payout processed for {request.AppointmentIds.Count} appointments");
        }
    }
}
