using TelePsy.BLL.Interfaces;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;


namespace TelePsy.BLL.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MatchingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Psychologist>> GetMatchesForPatientAsync(int patientId)
        {
            var patient = await _unitOfWork.Repository<Patient>()
                .GetFirstOrDefaultAsync(p => p.Id == patientId, includeProperties: "Person");
            if (patient == null) throw new Exception("Patient not found");

            var psychologists = await _unitOfWork.Repository<Psychologist>().GetAsync(includeProperties: "Person");

            // Simple matching logic: filtered by City, Gender preference

            var matches = psychologists.Where(p =>
                (string.IsNullOrEmpty(patient.PreferredGender) || p.Person.Gender == patient.PreferredGender) &&
                (string.IsNullOrEmpty(p.Person.City) ||
                 p.Person.City.Contains(patient.Person.City ?? "", StringComparison.OrdinalIgnoreCase))
            );

            // Refined matching:
            var scoredMatches = psychologists.Select(p => new
                {
                    Psychologist = p,
                    Score = CalculateMatchScore(patient, p)
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Psychologist);

            return scoredMatches;
        }

        private int CalculateMatchScore(Patient patient, Psychologist psychologist)
        {
            int score = 0;

            // City match (if patient had city, but currently only psychologist has city. Let's assume patient might have location data in future or pass as param)

            // Hobbies/Interests Match
            if (!string.IsNullOrEmpty(patient.Interests) && !string.IsNullOrEmpty(psychologist.Hobbies))
            {
                var patientInterests = patient.Interests.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim().ToLower());
                var psychHobbies = psychologist.Hobbies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => h.Trim().ToLower());

                var common = patientInterests.Intersect(psychHobbies).Count();
                score += common * 10;
            }

            return score;
        }
    }
}
