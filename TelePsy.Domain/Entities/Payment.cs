using System;

namespace TelePsy.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // Pending, Completed, Failed, Refunded
        public string TransactionId { get; set; } // PayU Transaction ID

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public int? PatientInvoiceId { get; set; }
        public Invoice PatientInvoice { get; set; }
    }
}
