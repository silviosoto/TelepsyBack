using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.DTOs;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class PsychologistService : IPsychologistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public PsychologistService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<Psychologist?> GetPsychologistByIdAsync(int id)
        {
            return (await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => p.Id == id,
                includeProperties: "Person"
            )).FirstOrDefault();
        }

        public async Task<Psychologist?> GetPsychologistByUserIdAsync(string userId)
        {
            return (await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => p.Person.UserId == userId,
                includeProperties: "Person"
            )).FirstOrDefault();
        }

        public async Task UpdateProfileAsync(int psychologistId, PsychologistProfileDto dto)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            psychologist.LicenseNumber = dto.LicenseNumber;
            psychologist.Specialization = dto.Specialization;
            psychologist.University = dto.University;
            psychologist.ExperienceYears = dto.ExperienceYears;
            psychologist.SessionRate = dto.SessionRate;
            psychologist.Bio = dto.Bio;
            psychologist.Hobbies = dto.Hobbies;

            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Psychologist>> GetAllPsychologistsAsync()
        {
            return await _unitOfWork.Repository<Psychologist>().GetAsync(
                includeProperties: "Person"
            );
        }

        public async Task<IEnumerable<Psychologist>> GetVerifiedPsychologistsAsync()
        {
            return await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => p.IsVerified && p.IsActive,
                includeProperties: "Person"
            );
        }

        public async Task UploadCvAsync(int psychologistId, Stream fileStream, string fileName)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            // Delete old file if exists
            if (!string.IsNullOrEmpty(psychologist.CvPath))
            {
                await _fileStorageService.DeleteFileAsync(psychologist.CvPath, "cvs");
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            psychologist.CvPath = await _fileStorageService.SaveFileAsync(fileStream, uniqueFileName, "cvs");

            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UploadProfilePictureAsync(int psychologistId, Stream fileStream, string fileName)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            // Delete old file if exists
            if (!string.IsNullOrEmpty(psychologist.ProfilePicturePath))
            {
                await _fileStorageService.DeleteFileAsync(psychologist.ProfilePicturePath, "profile-pictures");
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            psychologist.ProfilePicturePath =
                await _fileStorageService.SaveFileAsync(fileStream, uniqueFileName, "profile-pictures");

            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Therapy>> GetAvailableTherapiesAsync(string? searchQuery = null, int? limit = null)
        {
            var therapies = await _unitOfWork.Repository<Therapy>().GetAsync(t => t.IsActive);
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                therapies = therapies.Where(t => t.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || 
                                                 t.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }
            
            if (limit.HasValue)
            {
                therapies = therapies.Take(limit.Value);
            }
            
            return therapies;
        }

        public async Task<IEnumerable<PsychologistTherapyDto>> GetPsychologistServicesAsync(int psychologistId)
        {
            var services = await _unitOfWork.Repository<PsychologistTherapy>().GetAsync(
                pt => pt.PsychologistId == psychologistId,
                includeProperties: "Therapy"
            );

            return services.Select(s => new PsychologistTherapyDto
            {
                Id = s.Id,
                PsychologistId = s.PsychologistId,
                TherapyId = s.TherapyId,
                TherapyName = s.Therapy?.Name ?? string.Empty,
                TherapyDescription = s.Therapy?.Description ?? string.Empty,
                Rate = s.Rate,
                IsActive = s.IsActive
            });
        }

        public async Task UpdatePsychologistServiceAsync(int psychologistId, UpdatePsychologistServiceDto dto)
        {
            var existing = (await _unitOfWork.Repository<PsychologistTherapy>().GetAsync(
                pt => pt.PsychologistId == psychologistId && pt.TherapyId == dto.TherapyId
            )).FirstOrDefault();

            if (existing != null)
            {
                existing.Rate = dto.Rate;
                existing.IsActive = dto.IsActive;
                _unitOfWork.Repository<PsychologistTherapy>().Update(existing);
            }
            else
            {
                var newService = new PsychologistTherapy
                {
                    PsychologistId = psychologistId,
                    TherapyId = dto.TherapyId,
                    Rate = dto.Rate,
                    IsActive = dto.IsActive
                };
                await _unitOfWork.Repository<PsychologistTherapy>().AddAsync(newService);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<PatientListItemDto>> GetPatientsByPsychologistAsync(int psychologistId)
        {
            var appointments = await _unitOfWork.Repository<Appointment>().GetAsync(
                a => a.PsychologistId == psychologistId,
                includeProperties: "Patient,Patient.Person,Patient.Person.User"
            );

            return appointments
                .GroupBy(a => a.PatientId)
                .Select(g => 
                {
                    var firstAppt = g.First();
                    var patient = firstAppt.Patient;
                    var latestAppt = g.OrderByDescending(a => a.ScheduledTime).First();
                    
                    return new PatientListItemDto
                    {
                        Id = patient.Id,
                        FullName = patient.Person != null ? $"{patient.Person.FirstName} {patient.Person.LastName}".Trim() : "Unknown",
                        Email = patient.Person?.User?.Email,
                        Phone = patient.Person?.PhoneNumber,
                        ProfilePicturePath = null,
                        LastAppointmentDate = latestAppt.ScheduledTime,
                        SessionCount = g.Count()
                    };
                }).ToList();
        }

        public async Task<IEnumerable<SpecialtyDto>> GetAvailableSpecialtiesAsync(string? searchQuery = null, int? limit = null)
        {
            var specialties = await _unitOfWork.Repository<Specialty>().GetAsync(s => s.IsActive);
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                specialties = specialties.Where(s => s.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || 
                                                     (s.Description != null && s.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)));
            }
            
            if (limit.HasValue)
            {
                specialties = specialties.Take(limit.Value);
            }
            
            return specialties.Select(s => new SpecialtyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description ?? string.Empty
            });
        }

        public async Task<IEnumerable<SpecialtyDto>> GetPsychologistSpecialtiesAsync(int psychologistId)
        {
            var specialties = await _unitOfWork.Repository<PsychologistSpecialty>().GetAsync(
                ps => ps.PsychologistId == psychologistId && ps.IsActive,
                includeProperties: "Specialty"
            );

            return specialties.Select(ps => new SpecialtyDto
            {
                Id = ps.SpecialtyId,
                Name = ps.Specialty?.Name ?? "Unknown",
                Description = ps.Specialty?.Description ?? string.Empty
            });
        }

        public async Task UpdatePsychologistSpecialtyAsync(int psychologistId, UpdatePsychologistSpecialtyDto dto)
        {
            var existing = (await _unitOfWork.Repository<PsychologistSpecialty>().GetAsync(
                ps => ps.PsychologistId == psychologistId && ps.SpecialtyId == dto.SpecialtyId
            )).FirstOrDefault();

            if (existing != null)
            {
                existing.IsActive = dto.IsActive;
                _unitOfWork.Repository<PsychologistSpecialty>().Update(existing);
            }
            else if (dto.IsActive)
            {
                var newSpecialty = new PsychologistSpecialty
                {
                    PsychologistId = psychologistId,
                    SpecialtyId = dto.SpecialtyId,
                    IsActive = true
                };
                await _unitOfWork.Repository<PsychologistSpecialty>().AddAsync(newSpecialty);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
