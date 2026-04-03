using System;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PatientProfileDto> GetProfileAsync(string userId)
        {
            var person = (await _unitOfWork.Repository<Person>().GetAsync(p => p.UserId == userId)).FirstOrDefault();
            if (person == null)
            {
                return new PatientProfileDto();
            }

            var patient = (await _unitOfWork.Repository<Patient>().GetAsync(p => p.PersonId == person.Id)).FirstOrDefault();

            return new PatientProfileDto
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                DocumentType = person.DocumentType,
                DocumentNumber = person.DocumentNumber,
                City = person.City,
                Hobbies = patient?.Interests
            };
        }

        public async Task UpdateProfileAsync(string userId, PatientProfileDto dto)
        {
            var person = (await _unitOfWork.Repository<Person>().GetAsync(p => p.UserId == userId)).FirstOrDefault();
            
            if (person == null)
            {
                throw new Exception("Patient not found.");
            }

            person.FirstName = dto.FirstName ?? string.Empty;
            person.LastName = dto.LastName ?? string.Empty;
            if (dto.DateOfBirth.HasValue) 
            {
                person.DateOfBirth = dto.DateOfBirth.Value;
            }
            person.Gender = dto.Gender ?? string.Empty;
            person.DocumentType = dto.DocumentType;
            person.DocumentNumber = dto.DocumentNumber;
            person.City = dto.City ?? string.Empty;

            _unitOfWork.Repository<Person>().Update(person);

            var patient = (await _unitOfWork.Repository<Patient>().GetAsync(p => p.PersonId == person.Id)).FirstOrDefault();
            if (patient != null)
            {
                patient.Interests = dto.Hobbies ?? string.Empty;
                _unitOfWork.Repository<Patient>().Update(patient);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
