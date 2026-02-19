using System;
using System.Collections.Generic;
using TelePsy.Domain.Enums;

namespace TelePsy.Domain.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } // INV-YYYYMMDD-XXX
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public InvoiceType Type { get; set; }
        public InvoiceStatus Status { get; set; }

        // For Patient Invoices
        public int? PatientId { get; set; }
        public Patient Patient { get; set; }

        public int? PaymentId { get; set; }
        public Payment Payment { get; set; }

        // For Psychologist Payouts
        public int? PsychologistId { get; set; }
        public Psychologist Psychologist { get; set; }

        public ICollection<InvoiceDetail> Details { get; set; }
    }
}
