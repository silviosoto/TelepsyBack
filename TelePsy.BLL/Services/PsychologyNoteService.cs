using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class PsychologyNoteService : IPsychologyNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditService _auditService;

        public PsychologyNoteService(IUnitOfWork unitOfWork, IEncryptionService encryptionService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _auditService = auditService;
        }

        public async Task CreateNoteAsync(PsychologyNote note)
        {
            // Encrypt sensitive fields
            note.ReasonForSession = _encryptionService.Encrypt(note.ReasonForSession ?? "");
            note.FocusedMentalExam = _encryptionService.Encrypt(note.FocusedMentalExam ?? "");
            note.Evolution = _encryptionService.Encrypt(note.Evolution ?? "");
            note.Interventions = _encryptionService.Encrypt(note.Interventions ?? "");
            note.TherapeuticPlan = _encryptionService.Encrypt(note.TherapeuticPlan ?? "");

            await _unitOfWork.Repository<PsychologyNote>().AddAsync(note);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync(
                note.PsychologistId.ToString(),
                "Create",
                "PsychologyNote",
                note.Id.ToString(),
                $"Creada nota de evolución #{note.SessionNumber} para paciente {note.PatientId}"
            );
        }

        public async Task UpdateNoteAsync(PsychologyNote note)
        {
            var existingNote = await _unitOfWork.Repository<PsychologyNote>().GetByIdAsync(note.Id);
            if (existingNote == null || existingNote.PsychologistId != note.PsychologistId)
                throw new Exception("Nota de sesión no encontrada o sin permisos.");

            existingNote.SessionNumber = note.SessionNumber;
            existingNote.ServiceAttentionRecordId = note.ServiceAttentionRecordId ?? existingNote.ServiceAttentionRecordId;
            existingNote.NextAppointmentDate = note.NextAppointmentDate;
            existingNote.ProfessionalSignature = note.ProfessionalSignature ?? existingNote.ProfessionalSignature;

            // Encrypt updated sensitive fields
            existingNote.ReasonForSession = _encryptionService.Encrypt(note.ReasonForSession ?? "");
            existingNote.FocusedMentalExam = _encryptionService.Encrypt(note.FocusedMentalExam ?? "");
            existingNote.Evolution = _encryptionService.Encrypt(note.Evolution ?? "");
            existingNote.Interventions = _encryptionService.Encrypt(note.Interventions ?? "");
            existingNote.TherapeuticPlan = _encryptionService.Encrypt(note.TherapeuticPlan ?? "");

            _unitOfWork.Repository<PsychologyNote>().Update(existingNote);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync(
                note.PsychologistId.ToString(),
                "Update",
                "PsychologyNote",
                note.Id.ToString(),
                $"Actualizada nota de evolución #{note.SessionNumber}"
            );
        }

        public async Task<IEnumerable<PsychologyNote>> GetNotesForPatientAsync(int patientId, int psychologistId)
        {
            var notes = await _unitOfWork.Repository<PsychologyNote>()
                .GetAsync(
                    n => n.PatientId == patientId && n.PsychologistId == psychologistId,
                    includeProperties: "Appointment.Therapy"
                );

            var list = notes.OrderBy(n => n.SessionNumber).ThenBy(n => n.Date).ToList();
            foreach (var note in list)
            {
                DecryptNote(note);
            }

            return list;
        }

        public async Task<IEnumerable<PsychologyNote>> GetNotesForAttentionRecordAsync(int serviceAttentionRecordId)
        {
            var notes = await _unitOfWork.Repository<PsychologyNote>()
                .GetAsync(
                    n => n.ServiceAttentionRecordId == serviceAttentionRecordId,
                    includeProperties: "Appointment.Therapy,Psychologist.Person"
                );

            var list = notes.OrderBy(n => n.SessionNumber).ThenBy(n => n.Date).ToList();
            foreach (var note in list)
            {
                DecryptNote(note);
            }

            return list;
        }

        public async Task<PsychologyNote?> GetNoteByAppointmentIdAsync(int appointmentId)
        {
            var notes = await _unitOfWork.Repository<PsychologyNote>()
                .GetAsync(
                    n => n.AppointmentId == appointmentId,
                    includeProperties: "Appointment.Therapy,Psychologist.Person"
                );

            var note = notes.FirstOrDefault();
            if (note == null) return null;

            DecryptNote(note);
            return note;
        }

        public async Task<PsychologyNote?> GetByIdAsync(int id)
        {
            var notes = await _unitOfWork.Repository<PsychologyNote>()
                .GetAsync(
                    n => n.Id == id,
                    includeProperties: "Appointment.Therapy,Psychologist.Person"
                );

            var note = notes.FirstOrDefault();
            if (note == null) return null;

            DecryptNote(note);
            return note;
        }

        private void DecryptNote(PsychologyNote note)
        {
            try { note.ReasonForSession = !string.IsNullOrEmpty(note.ReasonForSession) ? _encryptionService.Decrypt(note.ReasonForSession) : ""; } catch { }
            try { note.FocusedMentalExam = !string.IsNullOrEmpty(note.FocusedMentalExam) ? _encryptionService.Decrypt(note.FocusedMentalExam) : ""; } catch { }
            try { note.Evolution = !string.IsNullOrEmpty(note.Evolution) ? _encryptionService.Decrypt(note.Evolution) : ""; } catch { }
            try { note.Interventions = !string.IsNullOrEmpty(note.Interventions) ? _encryptionService.Decrypt(note.Interventions) : ""; } catch { }
            try { note.TherapeuticPlan = !string.IsNullOrEmpty(note.TherapeuticPlan) ? _encryptionService.Decrypt(note.TherapeuticPlan) : ""; } catch { }
        }
    }
}
