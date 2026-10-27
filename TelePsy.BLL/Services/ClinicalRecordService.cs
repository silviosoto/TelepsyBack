using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class ClinicalRecordService : IClinicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditService _auditService;

        public ClinicalRecordService(IUnitOfWork unitOfWork, IEncryptionService encryptionService,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _auditService = auditService;
        }

        public async Task CreateRecordAsync(ClinicalRecord record)
        {
            // Encrypt sensitive data
            record.Notes = _encryptionService.Encrypt(record.Notes ?? "");
            record.Diagnosis = _encryptionService.Encrypt(record.Diagnosis ?? "");
            record.TreatmentPlan = _encryptionService.Encrypt(record.TreatmentPlan ?? "");

            await _unitOfWork.Repository<ClinicalRecord>().AddAsync(record);
            await _unitOfWork.CompleteAsync();

            // Audit
            await _auditService.LogAsync(record.PsychologistId.ToString(), "Create", "ClinicalRecord",
                record.Id.ToString(), "Created new clinical record");
        }

        public async Task<ClinicalRecord> CreateOrUpdateRecordAsync(ClinicalRecord record)
        {
            // Ensure 1 clinical history per patient: update if exists, otherwise create
            var existingRecords = await _unitOfWork.Repository<ClinicalRecord>().FindAsync(r => r.PatientId == record.PatientId);
            var existing = existingRecords.OrderByDescending(r => r.Date).FirstOrDefault();

            if (existing != null)
            {
                existing.PsychologistId = record.PsychologistId;
                existing.Date = DateTime.UtcNow;
                existing.Diagnosis = _encryptionService.Encrypt(record.Diagnosis ?? "");
                existing.TreatmentPlan = _encryptionService.Encrypt(record.TreatmentPlan ?? "");
                existing.Notes = _encryptionService.Encrypt(record.Notes ?? "");

                _unitOfWork.Repository<ClinicalRecord>().Update(existing);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogAsync(record.PsychologistId.ToString(), "Update", "ClinicalRecord",
                    existing.Id.ToString(), $"Updated clinical record for patient {record.PatientId}");

                return existing;
            }
            else
            {
                record.Date = DateTime.UtcNow;
                record.Notes = _encryptionService.Encrypt(record.Notes ?? "");
                record.Diagnosis = _encryptionService.Encrypt(record.Diagnosis ?? "");
                record.TreatmentPlan = _encryptionService.Encrypt(record.TreatmentPlan ?? "");

                await _unitOfWork.Repository<ClinicalRecord>().AddAsync(record);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogAsync(record.PsychologistId.ToString(), "Create", "ClinicalRecord",
                    record.Id.ToString(), $"Created new clinical record for patient {record.PatientId}");

                return record;
            }
        }

        public async Task<IEnumerable<ClinicalRecord>> GetRecordsForPatientAsync(int patientId)
        {
            var records = await _unitOfWork.Repository<ClinicalRecord>().FindAsync(r => r.PatientId == patientId);

            // Decrypt data
            foreach (var record in records)
            {
                record.Notes = !string.IsNullOrEmpty(record.Notes) ? _encryptionService.Decrypt(record.Notes) : "";
                record.Diagnosis = !string.IsNullOrEmpty(record.Diagnosis) ? _encryptionService.Decrypt(record.Diagnosis) : "";
                record.TreatmentPlan = !string.IsNullOrEmpty(record.TreatmentPlan) ? _encryptionService.Decrypt(record.TreatmentPlan) : "";
            }

            return records;
        }

        public async Task<ClinicalRecord?> GetLatestRecordForPatientAsync(int patientId)
        {
            var records = await _unitOfWork.Repository<ClinicalRecord>().FindAsync(r => r.PatientId == patientId);
            var latest = records.OrderByDescending(r => r.Date).FirstOrDefault();

            if (latest == null) return null;

            latest.Notes = !string.IsNullOrEmpty(latest.Notes) ? _encryptionService.Decrypt(latest.Notes) : "";
            latest.Diagnosis = !string.IsNullOrEmpty(latest.Diagnosis) ? _encryptionService.Decrypt(latest.Diagnosis) : "";
            latest.TreatmentPlan = !string.IsNullOrEmpty(latest.TreatmentPlan) ? _encryptionService.Decrypt(latest.TreatmentPlan) : "";

            return latest;
        }

        public async Task<bool> HasHistoryForPatientAsync(int patientId)
        {
            var records = await _unitOfWork.Repository<ClinicalRecord>().FindAsync(r => r.PatientId == patientId);
            return records.Any();
        }
    }
}
