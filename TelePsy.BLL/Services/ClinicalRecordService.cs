using System.Collections.Generic;
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
            record.Notes = _encryptionService.Encrypt(record.Notes);
            record.Diagnosis = _encryptionService.Encrypt(record.Diagnosis);
            record.TreatmentPlan = _encryptionService.Encrypt(record.TreatmentPlan);

            await _unitOfWork.Repository<ClinicalRecord>().AddAsync(record);
            await _unitOfWork.CompleteAsync();

            // Audit
            await _auditService.LogAsync(record.PsychologistId.ToString(), "Create", "ClinicalRecord",
                record.Id.ToString(), "Created new clinical record");
        }

        public async Task<IEnumerable<ClinicalRecord>> GetRecordsForPatientAsync(int patientId)
        {
            var records = await _unitOfWork.Repository<ClinicalRecord>().FindAsync(r => r.PatientId == patientId);

            // Decrypt data
            foreach (var record in records)
            {
                record.Notes = _encryptionService.Decrypt(record.Notes);
                record.Diagnosis = _encryptionService.Decrypt(record.Diagnosis);
                record.TreatmentPlan = _encryptionService.Decrypt(record.TreatmentPlan);
            }

            // Audit read access
            // await _auditService.LogAsync(..., "Read", ...); // Need current user ID context here ideally

            return records;
        }
    }
}
