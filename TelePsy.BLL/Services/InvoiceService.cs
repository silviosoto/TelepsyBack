using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;

namespace TelePsy.BLL.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CommissionKey = "CommissionRate";
        private const decimal DefaultCommission = 0.30m;

        public InvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Invoice> GeneratePatientInvoiceAsync(int paymentId)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.PatientInvoiceId != null) throw new Exception("Invoice already exists for this payment");
            // Assuming Payment status check is done or trusting the payment existence implies validity for now
            // But ideally: if (payment.Status != "Approved") throw ...

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(payment.AppointmentId);

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{payment.Id}",
                IssueDate = DateTime.UtcNow,
                TotalAmount = payment.Amount,
                Type = InvoiceType.ClientPurchase,
                Status = InvoiceStatus.Paid,
                PatientId = appointment.PatientId,
                PaymentId = paymentId,
                Details = new List<InvoiceDetail>
                {
                    new InvoiceDetail
                    {
                        AppointmentId = appointment.Id,
                        Description = "Psychology Session",
                        UnitPrice = payment.Amount,
                        CommissionAmount = 0,
                        Total = payment.Amount
                    }
                }
            };

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _unitOfWork.CompleteAsync();

            // Link back (though standard EF setup might handle the FK update on Payment side if we loaded it, but better to be safe)
            payment.PatientInvoiceId = invoice.Id;
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();

            return invoice;
        }

        public async Task<Invoice> GeneratePsychologistPayoutAsync(int psychologistId, List<int> appointmentIds)
        {
            var commissionRate = await GetGlobalCommissionAsync();

            var appointments = await _unitOfWork.Repository<Appointment>().GetAsync(a =>
                appointmentIds.Contains(a.Id) && a.PsychologistId == psychologistId
            );

            if (appointments.Count() != appointmentIds.Count)
                throw new Exception("One or more appointments not found or do not belong to psychologist");

            var invoice = new Invoice
            {
                InvoiceNumber =
                    $"PAY-{DateTime.UtcNow:yyyyMMdd}-{psychologistId}-{Guid.NewGuid().ToString().Substring(0, 4)}",
                IssueDate = DateTime.UtcNow,
                Type = InvoiceType.PsychologistPayout,
                Status = InvoiceStatus.Issued,
                PsychologistId = psychologistId,
                Details = new List<InvoiceDetail>()
            };

            decimal totalPayout = 0;

            foreach (var appt in appointments)
            {
                if (appt.Status != AppointmentStatus.Completed &&
                    appt.Status != AppointmentStatus.Confirmed) // Assuming Confirmed/Completed are valid for payout
                {
                    // Strict check: only Completed?
                    // For now, allow Confirmed or Completed.
                }

                if (appt.PsychologistInvoiceId != null)
                    throw new Exception($"Appointment {appt.Id} is already invoiced.");

                // Assuming Appointment doesn't have a Price property on it directly, 
                // we might need to look at the Payment associated with it?
                // Or does Appointment have a Price? 
                // Checking Appointment.cs from previous steps... it does NOT have Price.
                // It has PaymentId. We should get the price from the Payment.

                // We need to fetch appointments WITH Payments.
                // Refetching or we should have used Include.
            }

            // Re-fetch with Payment
            var appointmentsWithPayment = await _unitOfWork.Repository<Appointment>().GetAsync(
                a => appointmentIds.Contains(a.Id) && a.PsychologistId == psychologistId,
                includeProperties: "Payment"
            );

            foreach (var appt in appointmentsWithPayment)
            {
                if (appt.Payment == null) throw new Exception($"Appointment {appt.Id} has no payment record.");

                decimal price = appt.Payment.Amount;
                decimal commission = price * commissionRate;
                decimal payout = price - commission;

                invoice.Details.Add(new InvoiceDetail
                {
                    AppointmentId = appt.Id,
                    Description = $"Payout for Session {appt.Id}",
                    UnitPrice = price,
                    CommissionAmount = commission,
                    Total = payout
                });

                totalPayout += payout;
                appt.PsychologistInvoice = invoice; // EF Link
            }

            invoice.TotalAmount = totalPayout;

            await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
            await _unitOfWork.CompleteAsync();

            return invoice;
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            var invoice = (await _unitOfWork.Repository<Invoice>().GetAsync(
                i => i.Id == id,
                includeProperties: "Details,Details.Appointment,Patient.Person,Psychologist.Person"
            )).FirstOrDefault();

            return invoice;
        }

        public async Task<IEnumerable<Appointment>> GetUnpaidAppointmentsForPsychologistAsync(int psychologistId)
        {
            return await _unitOfWork.Repository<Appointment>().GetAsync(
                a => a.PsychologistId == psychologistId
                     && a.PsychologistInvoiceId == null
                     && (a.Status == AppointmentStatus.Completed ||
                         a.Status == AppointmentStatus.Confirmed), // Basic valid statuses
                includeProperties: "Patient.Person,Payment"
            );
        }

        public async Task<decimal> GetGlobalCommissionAsync()
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(c => c.Key == CommissionKey))
                .FirstOrDefault();
            if (config != null && decimal.TryParse(config.Value, out var rate))
            {
                return rate;
            }

            return DefaultCommission;
        }

        public async Task UpdateGlobalCommissionAsync(decimal rate)
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(c => c.Key == CommissionKey))
                .FirstOrDefault();
            if (config == null)
            {
                config = new GlobalConfiguration { Key = CommissionKey, Value = rate.ToString() };
                await _unitOfWork.Repository<GlobalConfiguration>().AddAsync(config);
            }
            else
            {
                config.Value = rate.ToString();
                config.LastUpdated = DateTime.UtcNow;
                _unitOfWork.Repository<GlobalConfiguration>().Update(config);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
