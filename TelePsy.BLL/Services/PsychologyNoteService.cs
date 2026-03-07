using System.Collections.Generic;
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
            note.ReasonForSession = _encryptionService.Encrypt(note.ReasonForSession);
            note.Evolution = _encryptionService.Encrypt(note.Evolution);
            note.Interventions = _encryptionService.Encrypt(note.Interventions);
            note.TherapeuticPlan = _encryptionService.Encrypt(note.TherapeuticPlan);

            await _unitOfWork.Repository<PsychologyNote>().AddAsync(note);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync(note.PsychologistId.ToString(), "Create", "PsychologyNote", note.Id.ToString(), "Created new session note");
        }

        public async Task<IEnumerable<PsychologyNote>> GetNotesForPatientAsync(int patientId, int psychologistId)
        {
            // Usually notes might be shared, or strict per psychologist. For now, pull notes for that patient that this psychologist has access to.
            var notes = await _unitOfWork.Repository<PsychologyNote>()
                    .FindAsync(n => n.PatientId == patientId && n.PsychologistId == psychologistId);

            foreach (var note in notes)
            {
                note.ReasonForSession = _encryptionService.Decrypt(note.ReasonForSession);
                note.Evolution = _encryptionService.Decrypt(note.Evolution);
                note.Interventions = _encryptionService.Decrypt(note.Interventions);
                note.TherapeuticPlan = _encryptionService.Decrypt(note.TherapeuticPlan);
            }

            return notes;
        }
    }
}
