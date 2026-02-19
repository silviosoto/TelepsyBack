using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;

namespace TelePsy.BLL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AdminService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Psychologist>> GetPendingPsychologistsAsync()
        {
            return await _unitOfWork.Repository<Psychologist>().GetAsync(
                p => !p.IsVerified && p.IsActive,
                includeProperties: "Person"
            );
        }

        public async Task ApprovePsychologistAsync(int psychologistId)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            psychologist.IsVerified = true;
            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Approve", "Psychologist", 
                psychologistId.ToString(), "Psychologist approved");
        }

        public async Task RejectPsychologistAsync(int psychologistId, string reason)
        {
            var psychologist = await _unitOfWork.Repository<Psychologist>().GetByIdAsync(psychologistId);
            if (psychologist == null) throw new Exception("Psychologist not found");

            psychologist.IsActive = false;
            _unitOfWork.Repository<Psychologist>().Update(psychologist);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Reject", "Psychologist", 
                psychologistId.ToString(), $"Psychologist rejected: {reason}");
        }

        public async Task<decimal> GetCommissionRateAsync()
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(
                c => c.Key == "CommissionRate"
            )).FirstOrDefault();

            if (config != null && decimal.TryParse(config.Value, out var rate))
            {
                return rate;
            }

            return 0.30m; // Default
        }

        public async Task UpdateCommissionRateAsync(decimal rate)
        {
            var config = (await _unitOfWork.Repository<GlobalConfiguration>().GetAsync(
                c => c.Key == "CommissionRate"
            )).FirstOrDefault();

            if (config == null)
            {
                config = new GlobalConfiguration
                {
                    Key = "CommissionRate",
                    Value = rate.ToString()
                };
                await _unitOfWork.Repository<GlobalConfiguration>().AddAsync(config);
            }
            else
            {
                config.Value = rate.ToString();
                config.LastUpdated = DateTime.UtcNow;
                _unitOfWork.Repository<GlobalConfiguration>().Update(config);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogAsync("Admin", "Update", "GlobalConfiguration", 
                "CommissionRate", $"Commission rate updated to {rate}");
        }
    }
}
