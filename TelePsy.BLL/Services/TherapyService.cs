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
    public class TherapyService : ITherapyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TherapyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TherapyDto>> GetAllAsync()
        {
            var therapies = await _unitOfWork.Repository<Therapy>().GetAllAsync();
            return therapies.Select(t => new TherapyDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsActive = t.IsActive
            });
        }

        public async Task<TherapyDto> GetByIdAsync(int id)
        {
            var therapy = await _unitOfWork.Repository<Therapy>().GetByIdAsync(id);
            if (therapy == null) return null;

            return new TherapyDto
            {
                Id = therapy.Id,
                Name = therapy.Name,
                Description = therapy.Description,
                IsActive = therapy.IsActive
            };
        }

        public async Task<Therapy> CreateAsync(CreateTherapyDto dto)
        {
            var therapy = new Therapy
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };

            await _unitOfWork.Repository<Therapy>().AddAsync(therapy);
            await _unitOfWork.CompleteAsync();

            return therapy;
        }

        public async Task UpdateAsync(int id, CreateTherapyDto dto)
        {
            var therapy = await _unitOfWork.Repository<Therapy>().GetByIdAsync(id);
            if (therapy == null) throw new Exception("Therapy not found");

            therapy.Name = dto.Name;
            therapy.Description = dto.Description;

            _unitOfWork.Repository<Therapy>().Update(therapy);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ToggleStatusAsync(int id)
        {
            var therapy = await _unitOfWork.Repository<Therapy>().GetByIdAsync(id);
            if (therapy == null) throw new Exception("Therapy not found");

            therapy.IsActive = !therapy.IsActive;

            _unitOfWork.Repository<Therapy>().Update(therapy);
            await _unitOfWork.CompleteAsync();
        }
    }
}
