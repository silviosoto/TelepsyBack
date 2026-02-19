using System;

namespace TelePsy.Domain.Entities
{
    public class InvoiceDetail
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        
        public int? AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CommissionAmount { get; set; } // 0 for Patient Invoice
        public decimal Total { get; set; } // UnitPrice - CommissionAmount (for Payout) or UnitPrice (for Patient)
    }
}
