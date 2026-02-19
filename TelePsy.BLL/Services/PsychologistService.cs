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
    }
}
