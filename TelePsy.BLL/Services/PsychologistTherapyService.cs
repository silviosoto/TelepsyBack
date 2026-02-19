using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class PsychologistTherapyService : IPsychologistTherapyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PsychologistTherapyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PsychologistTherapyDto>> GetByPsychologistIdAsync(int psychologistId)
        {
            var therapies = await _unitOfWork.Repository<PsychologistTherapy>().GetAsync(
                filter: pt => pt.PsychologistId == psychologistId && pt.IsActive,
                includeProperties: "Therapy"
            );

            return therapies.Select(pt => new PsychologistTherapyDto
            {
                Id = pt.Id,
                PsychologistId = pt.PsychologistId,
                TherapyId = pt.TherapyId,
                TherapyName = pt.Therapy?.Name ?? "Unknown",
                Rate = pt.Rate,
                IsActive = pt.IsActive
            });
        }

        public async Task SetRateAsync(int psychologistId, SetRateDto dto)
        {
            // Check if relationship already exists
            var existing = await _unitOfWork.Repository<PsychologistTherapy>().GetFirstOrDefaultAsync(
                pt => pt.PsychologistId == psychologistId && pt.TherapyId == dto.TherapyId
            );

            if (existing != null)
            {
                existing.Rate = dto.Rate;
                existing.IsActive = true; // Reactivate if it was disabled
                _unitOfWork.Repository<PsychologistTherapy>().Update(existing);
            }
            else
            {
                var newEntity = new PsychologistTherapy
                {
                    PsychologistId = psychologistId,
                    TherapyId = dto.TherapyId,
                    Rate = dto.Rate,
                    IsActive = true
                };
                await _unitOfWork.Repository<PsychologistTherapy>().AddAsync(newEntity);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveTherapyAsync(int psychologistId, int therapyId)
        {
            var existing = await _unitOfWork.Repository<PsychologistTherapy>().GetFirstOrDefaultAsync(
                pt => pt.PsychologistId == psychologistId && pt.TherapyId == therapyId
            );

            if (existing != null)
            {
                // Soft delete or hard delete? Let's do soft delete by setting IsActive = false
                // But typically "Removal" implies removing from the list. 
                // However, for historical data integrity, soft delete is safer.
                // But wait, if I never delete, the generic repository "Delete" method does hard delete.
                // My entities show `IsActive`. usage suggests soft delete.
                
                // Let's check `Delete` in `GenericRepository`:
                // It does `dbSet.Remove(entity)`. That is a hard delete.
                // But I should use IsActive = false if I want to keep history?
                // Actually, PsychologistTherapy is just a configuration. If used in Appointments, FK will prevent hard delete if restrict/no-action.
                // Domain: `OnDelete(DeleteBehavior.Restrict)` for Therapy->PsychologistTherapy? No.
                // `DeleteBehavior.Cascade` for Psychologist->Therapy.
                // Since I added `IsActive` to the entity, I should probably use it.
                existing.IsActive = false;
                _unitOfWork.Repository<PsychologistTherapy>().Update(existing);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
