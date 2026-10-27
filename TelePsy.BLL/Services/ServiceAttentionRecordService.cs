using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class ServiceAttentionRecordService : IServiceAttentionRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditService _auditService;

        private class EvaluationPayload
        {
            public PhysicalExamDto PhysicalExam { get; set; } = new();
            public RelatedFactorsDto RelatedFactors { get; set; } = new();
            public MentalStateExamDto MentalStateExam { get; set; } = new();
            public InitialConceptDto InitialConcept { get; set; } = new();
        }

        public ServiceAttentionRecordService(
            IUnitOfWork unitOfWork,
            IEncryptionService encryptionService,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _auditService = auditService;
        }

        public async Task<ServiceAttentionRecordDto?> GetActiveRecordForServiceAsync(int patientId, int therapyId)
        {
            var records = await _unitOfWork.Repository<ServiceAttentionRecord>().GetAsync(
                r => r.PatientId == patientId && r.TherapyId == therapyId,
                includeProperties: "Psychologist.Person,Therapy,EvolutionNotes"
            );

            var active = records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            if (active == null) return null;

            return MapToDto(active);
        }

        public async Task<IEnumerable<ServiceAttentionRecordDto>> GetRecordsForPatientAsync(int patientId)
        {
            var records = await _unitOfWork.Repository<ServiceAttentionRecord>().GetAsync(
                r => r.PatientId == patientId,
                includeProperties: "Psychologist.Person,Therapy,EvolutionNotes"
            );

            return records.OrderByDescending(r => r.CreatedAt).Select(MapToDto).ToList();
        }

        public async Task<ServiceAttentionRecordDto?> GetByIdAsync(int id)
        {
            var records = await _unitOfWork.Repository<ServiceAttentionRecord>().GetAsync(
                r => r.Id == id,
                includeProperties: "Psychologist.Person,Therapy,EvolutionNotes"
            );

            var record = records.FirstOrDefault();
            if (record == null) return null;

            return MapToDto(record);
        }

        public async Task<ServiceAttentionRecordDto> CreateRecordAsync(CreateServiceAttentionRecordDto dto, int psychologistId)
        {
            var payload = new EvaluationPayload
            {
                PhysicalExam = dto.PhysicalExam,
                RelatedFactors = dto.RelatedFactors,
                MentalStateExam = dto.MentalStateExam,
                InitialConcept = dto.InitialConcept
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var encryptedPayload = _encryptionService.Encrypt(jsonPayload);

            var record = new ServiceAttentionRecord
            {
                PatientId = dto.PatientId,
                PsychologistId = psychologistId,
                TherapyId = dto.TherapyId,
                InitialAppointmentId = dto.InitialAppointmentId,
                CreatedAt = DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "EnTratamiento" : dto.Status,
                EvaluationData = encryptedPayload,
                MainDiagnosis = _encryptionService.Encrypt(dto.InitialConcept?.MainDiagnosis ?? ""),
                TreatmentPlan = _encryptionService.Encrypt(dto.InitialConcept?.TreatmentPlan ?? "")
            };

            await _unitOfWork.Repository<ServiceAttentionRecord>().AddAsync(record);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync(
                psychologistId.ToString(),
                "Create",
                "ServiceAttentionRecord",
                record.Id.ToString(),
                $"Creada valoración inicial de servicio para paciente {dto.PatientId}, terapia {dto.TherapyId}"
            );

            return (await GetByIdAsync(record.Id))!;
        }

        public async Task<ServiceAttentionRecordDto> UpdateRecordAsync(int id, UpdateServiceAttentionRecordDto dto, int psychologistId)
        {
            var record = await _unitOfWork.Repository<ServiceAttentionRecord>().GetByIdAsync(id);
            if (record == null)
                throw new Exception("Registro de atención no encontrado.");

            var payload = new EvaluationPayload
            {
                PhysicalExam = dto.PhysicalExam,
                RelatedFactors = dto.RelatedFactors,
                MentalStateExam = dto.MentalStateExam,
                InitialConcept = dto.InitialConcept
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            record.EvaluationData = _encryptionService.Encrypt(jsonPayload);
            record.LastUpdatedAt = DateTime.UtcNow;
            record.Status = string.IsNullOrWhiteSpace(dto.Status) ? record.Status : dto.Status;
            record.MainDiagnosis = _encryptionService.Encrypt(dto.InitialConcept?.MainDiagnosis ?? "");
            record.TreatmentPlan = _encryptionService.Encrypt(dto.InitialConcept?.TreatmentPlan ?? "");

            _unitOfWork.Repository<ServiceAttentionRecord>().Update(record);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync(
                psychologistId.ToString(),
                "Update",
                "ServiceAttentionRecord",
                record.Id.ToString(),
                $"Actualizada valoración de servicio {record.Id}"
            );

            return (await GetByIdAsync(record.Id))!;
        }

        private ServiceAttentionRecordDto MapToDto(ServiceAttentionRecord record)
        {
            var dto = new ServiceAttentionRecordDto
            {
                Id = record.Id,
                PatientId = record.PatientId,
                PsychologistId = record.PsychologistId,
                PsychologistName = record.Psychologist?.Person != null 
                    ? $"{record.Psychologist.Person.FirstName} {record.Psychologist.Person.LastName}".Trim() 
                    : "",
                TherapyId = record.TherapyId,
                TherapyName = record.Therapy?.Name ?? "Terapia",
                InitialAppointmentId = record.InitialAppointmentId,
                CreatedAt = record.CreatedAt,
                LastUpdatedAt = record.LastUpdatedAt,
                Status = record.Status,
                EvolutionNotesCount = record.EvolutionNotes?.Count ?? 0
            };

            if (!string.IsNullOrEmpty(record.EvaluationData))
            {
                try
                {
                    var decryptedJson = _encryptionService.Decrypt(record.EvaluationData);
                    var payload = JsonSerializer.Deserialize<EvaluationPayload>(decryptedJson);
                    if (payload != null)
                    {
                        dto.PhysicalExam = payload.PhysicalExam ?? new();
                        dto.RelatedFactors = payload.RelatedFactors ?? new();
                        dto.MentalStateExam = payload.MentalStateExam ?? new();
                        dto.InitialConcept = payload.InitialConcept ?? new();
                    }
                }
                catch
                {
                    // If decryption or parsing fails, return empty objects
                }
            }

            return dto;
        }
    }
}
