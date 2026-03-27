using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TelePsy.Domain.Entities;

namespace TelePsy.DAL.Context
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Psychologist> Psychologists { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SessionPackage> SessionPackages { get; set; }
        public DbSet<ClinicalRecord> ClinicalRecords { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<BlockedSlot> BlockedSlots { get; set; }
        public DbSet<Therapy> Therapies { get; set; }
        public DbSet<PsychologistTherapy> PsychologistTherapies { get; set; }
        public DbSet<PsychologyNote> PsychologyNotes { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<PsychologistSpecialty> PsychologistSpecialties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Person>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Person)
                    .HasForeignKey<Person>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.City).HasMaxLength(100);
            });

            builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Psychologist)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PsychologistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
                entity.HasOne(a => a.Payment)
                    .WithOne(p => p.Appointment)
                    .HasForeignKey<Payment>(p => p.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.SessionPackage)
                    .WithMany(sp => sp.Appointments)
                    .HasForeignKey(a => a.SessionPackageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ClinicalRecord>(entity =>
            {
                entity.HasOne(c => c.Patient)
                    .WithMany(p => p.ClinicalRecords)
                    .HasForeignKey(c => c.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Diagnosis).IsRequired(); // Encrypted but required
            });

            builder.Entity<Patient>(entity =>
            {
                entity.HasOne(p => p.Person)
                    .WithMany()
                    .HasForeignKey(p => p.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Occupation).HasMaxLength(100);
            });

            builder.Entity<Psychologist>(entity =>
            {
                entity.HasOne(p => p.Person)
                    .WithMany()
                    .HasForeignKey(p => p.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            });

            builder.Entity<Admin>(entity =>
            {
                entity.HasOne(a => a.Person)
                    .WithMany()
                    .HasForeignKey(a => a.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Invoice>(entity =>
            {
                entity.HasOne(i => i.Patient)
                    .WithMany()
                    .HasForeignKey(i => i.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Psychologist)
                    .WithMany()
                    .HasForeignKey(i => i.PsychologistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Payment)
                    .WithOne(p => p.PatientInvoice)
                    .HasForeignKey<
                        Payment>(p =>
                        p.PatientInvoiceId) // Configuring 1:1, Invoice is principal side? Payment depends on Invoice?
                    // Actually, if Payment exists first, and Invoice is created later...
                    // Let's rely on manual setting or standard FK.
                    // If Payment has PatientInvoiceId, then Payment -> Invoice is the FK relationship.
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<InvoiceDetail>(entity =>
            {
                entity.HasOne(d => d.Invoice)
                    .WithMany(i => i.Details)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Appointment)
                    .WithMany() // Appointment doesn't need to know about details directly
                    .HasForeignKey(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SessionPackage Configuration
            builder.Entity<SessionPackage>(entity =>
            {
                entity.HasOne(sp => sp.Patient)
                    .WithMany()
                    .HasForeignKey(sp => sp.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sp => sp.Psychologist)
                    .WithMany()
                    .HasForeignKey(sp => sp.PsychologistId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sp => sp.Therapy)
                    .WithMany()
                    .HasForeignKey(sp => sp.TherapyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sp => sp.Payment)
                    .WithOne()
                    .HasForeignKey<SessionPackage>(sp => sp.PaymentId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(sp => sp.OriginalTotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(sp => sp.DiscountPercentage).HasColumnType("decimal(18,2)");
                entity.Property(sp => sp.FinalAmount).HasColumnType("decimal(18,2)");
            });

            // Therapy Configuration
            builder.Entity<Therapy>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            });

            // PsychologistTherapy Configuration
            builder.Entity<PsychologistTherapy>(entity =>
            {
                entity.HasOne(pt => pt.Psychologist)
                    .WithMany(p => p.Therapies)
                    .HasForeignKey(pt => pt.PsychologistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Therapy)
                    .WithMany(t => t.PsychologistTherapies)
                    .HasForeignKey(pt => pt.TherapyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pt => pt.Rate).HasColumnType("decimal(18,2)");
            });

            // Appointment Therapy Relation
            builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Therapy)
                    .WithMany()
                    .HasForeignKey(a => a.TherapyId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // Specialty Configurations
            builder.Entity<Specialty>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // PsychologistSpecialty Configuration
            builder.Entity<PsychologistSpecialty>(entity =>
            {
                entity.HasOne(ps => ps.Psychologist)
                    .WithMany(p => p.Specialties)
                    .HasForeignKey(ps => ps.PsychologistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Specialty)
                    .WithMany(s => s.PsychologistSpecialties)
                    .HasForeignKey(ps => ps.SpecialtyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Force UTC for all DateTime properties
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }
        }
    }
}
