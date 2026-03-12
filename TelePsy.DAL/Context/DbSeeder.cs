using Microsoft.AspNetCore.Identity;
using TelePsy.Domain.Entities;
using TelePsy.DAL.Context;
using Microsoft.Extensions.DependencyInjection;

namespace TelePsy.DAL.Context
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // Roles
            string[] roles = { "Admin", "Psychologist", "Patient" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Admin User
            string adminEmail = "admin@telepsy.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create Person for Admin if not exists
            var adminPerson = context.People.FirstOrDefault(p => p.UserId == adminUser.Id);
            if (adminPerson == null)
            {
                adminPerson = new Person
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    UserId = adminUser.Id,
                    IsActive = true,
                    PhoneNumber = "0000000000",
                    Address = "System",
                    City = "System",
                    State = "System",
                    Country = "System",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Gender = "Other"
                };
                context.People.Add(adminPerson);
                await context.SaveChangesAsync();
            }

            // Create Admin record if not exists
            var adminRecord = context.Admins.FirstOrDefault(a => a.PersonId == adminPerson.Id);
            if (adminRecord == null)
            {
                adminRecord = new Admin
                {
                    PersonId = adminPerson.Id,
                    IsActive = true,
                    Department = "IT Support"
                };
                context.Admins.Add(adminRecord);
                await context.SaveChangesAsync();
            }

            // Test Psychologist
            string psychEmail = "psych@telepsy.com";
            if (await userManager.FindByEmailAsync(psychEmail) == null)
            {
                var psychUser = new User { UserName = psychEmail, Email = psychEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(psychUser, "Psych123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(psychUser, "Psychologist");

                    var person = new Person 
                    { 
                        FirstName = "John", 
                        LastName = "Doe", 
                        UserId = psychUser.Id, 
                        IsActive = true,
                        PhoneNumber = "1234567890",
                        Address = "123 Psych St",
                        City = "Bogota",
                        State = "DC",
                        Country = "Colombia",
                        DateOfBirth = new DateTime(1980, 1, 1),
                        Gender = "Male"
                    };
                    context.People.Add(person);
                    await context.SaveChangesAsync();

                    var psychologist = new Psychologist 
                    { 
                        PersonId = person.Id, 
                        IsActive = true, 
                        LicenseNumber = "PSY12345",
                        Specialization = "Clinical Psychology",
                        ExperienceYears = 10,
                        Bio = "Senior psychologist with 10 years experience.",
                        University = "National University",
                        Hobbies = "Reading, Hiking",
                        SessionRate = 150000
                    };
                    context.Psychologists.Add(psychologist);
                    await context.SaveChangesAsync();
                }
            }

            // Test Patient
            string patientEmail = "patient@telepsy.com";
            if (await userManager.FindByEmailAsync(patientEmail) == null)
            {
                var patientUser = new User { UserName = patientEmail, Email = patientEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(patientUser, "Patient123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");

                    var person = new Person 
                    { 
                        FirstName = "Jane", 
                        LastName = "Smith", 
                        UserId = patientUser.Id, 
                        IsActive = true,
                        PhoneNumber = "0987654321",
                        Address = "456 Patient Rd",
                        City = "Medellin",
                        State = "Antioquia",
                        Country = "Colombia",
                        DateOfBirth = new DateTime(1995, 5, 15),
                        Gender = "Female"
                    };
                    context.People.Add(person);
                    await context.SaveChangesAsync();

                    var patient = new Patient 
                    { 
                        PersonId = person.Id, 
                        IsActive = true, 
                        Occupation = "Teacher",
                        EmergencyContact = "Mom: 555-1234",
                        PreferredGender = "No Preference",
                        Interests = "Music, Art"
                    };
                    context.Patients.Add(patient);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Locations
            await SeedLocationsAsync(context);

            // Seed Therapies and Specialties
            await SeedTherapiesAsync(context);
            await SeedSpecialtiesAsync(context);

            // Ensure test psychologist is verified and has services
            var john = context.Psychologists.FirstOrDefault(p => p.LicenseNumber == "PSY12345");
            if (john != null)
            {
                john.IsVerified = true;
                
                // Add sample therapy if none
                if (!context.PsychologistTherapies.Any(pt => pt.PsychologistId == john.Id))
                {
                    var therapy = context.Therapies.FirstOrDefault();
                    if (therapy != null)
                    {
                        context.PsychologistTherapies.Add(new PsychologistTherapy 
                        { 
                            PsychologistId = john.Id, 
                            TherapyId = therapy.Id, 
                            Rate = 120000 
                        });
                    }
                }

                // Add sample specialty if none
                if (!context.PsychologistSpecialties.Any(ps => ps.PsychologistId == john.Id))
                {
                    var specialty = context.Specialties.FirstOrDefault();
                    if (specialty != null)
                    {
                        context.PsychologistSpecialties.Add(new PsychologistSpecialty 
                        { 
                            PsychologistId = john.Id, 
                            SpecialtyId = specialty.Id, 
                            IsActive = true 
                        });
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTherapiesAsync(AppDbContext context)
        {
            if (context.Therapies.Any()) return;

            var therapies = new List<Therapy>
            {
                new Therapy { Name = "Terapia Individual", Description = "Sesión personalizada de 50 minutos." },
                new Therapy { Name = "Terapia de Pareja", Description = "Enfoque en comunicación y resolución de conflictos." },
                new Therapy { Name = "Terapia Infantil", Description = "Especializada para niños y adolescentes." },
                new Therapy { Name = "Orientación Vocacional", Description = "Ayuda para elegir carrera y futuro profesional." }
            };

            context.Therapies.AddRange(therapies);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSpecialtiesAsync(AppDbContext context)
        {
            if (context.Specialties.Any()) return;

            var specialties = new List<Specialty>
            {
                new Specialty { Name = "Ansiedad y Estrés", IsActive = true },
                new Specialty { Name = "Depresión", IsActive = true },
                new Specialty { Name = "Duelos", IsActive = true },
                new Specialty { Name = "Trastornos de Personalidad", IsActive = true }
            };

            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLocationsAsync(AppDbContext context)
        {
            if (context.Departments.Any()) return;

            var locations = new Dictionary<string, string[]>
            {
                { "Amazonas", new[] { "Leticia", "Puerto Nariño" } },
                { "Antioquia", new[] { "Medellín", "Bello", "Itagüí", "Envigado", "Rionegro", "Apartadó" } },
                { "Arauca", new[] { "Arauca", "Tame", "Saravena" } },
                { "Atlántico", new[] { "Barranquilla", "Soledad", "Malambo", "Sabanalarga" } },
                { "Bogotá D.C.", new[] { "Bogotá" } },
                { "Bolívar", new[] { "Cartagena", "Magangué", "Turbaco" } },
                { "Boyacá", new[] { "Tunja", "Duitama", "Sogamoso", "Chiquinquirá" } },
                { "Caldas", new[] { "Manizales", "La Dorada", "Riosucio" } },
                { "Caquetá", new[] { "Florencia", "San Vicente del Caguán" } },
                { "Casanare", new[] { "Yopal", "Aguazul", "Paz de Ariporo" } },
                { "Cauca", new[] { "Popayán", "Santander de Quilichao", "Puerto Tejada" } },
                { "Cesar", new[] { "Valledupar", "Aguachica", "Agustín Codazzi" } },
                { "Chocó", new[] { "Quibdó", "Istmina", "Condoto" } },
                { "Córdoba", new[] { "Montería", "Cereté", "Sahagún", "Lorica" } },
                { "Cundinamarca", new[] { "Soacha", "Fusagasugá", "Facatativá", "Chía", "Zipaquirá", "Girardot" } },
                { "Guainía", new[] { "Inírida" } },
                { "Guaviare", new[] { "San José del Guaviare" } },
                { "Huila", new[] { "Neiva", "Pitalito", "Garzón" } },
                { "La Guajira", new[] { "Riohacha", "Maicao", "Uribia" } },
                { "Magdalena", new[] { "Santa Marta", "Ciénaga", "Fundación" } },
                { "Meta", new[] { "Villavicencio", "Acacías", "Granada" } },
                { "Nariño", new[] { "Pasto", "Tumaco", "Ipiales" } },
                { "Norte de Santander", new[] { "Cúcuta", "Ocaña", "Villa del Rosario" } },
                { "Putumayo", new[] { "Mocoa", "Puerto Asís", "Orito" } },
                { "Quindío", new[] { "Armenia", "Calarcá", "La Tebaida" } },
                { "Risaralda", new[] { "Pereira", "Dosquebradas", "Santa Rosa de Cabal" } },
                { "San Andrés y Providencia", new[] { "San Andrés" } },
                { "Santander", new[] { "Bucaramanga", "Floridablanca", "Girón", "Piedecuesta", "Barrancabermeja" } },
                { "Sucre", new[] { "Sincelejo", "Corozal", "San Marcos" } },
                { "Tolima", new[] { "Ibagué", "Espinal", "Melgar" } },
                { "Valle del Cauca", new[] { "Cali", "Buenaventura", "Palmira", "Tuluá", "Cartago", "Buga" } },
                { "Vaupés", new[] { "Mitú" } },
                { "Vichada", new[] { "Puerto Carreño" } }
            };

            foreach (var loc in locations)
            {
                var department = new Department { Name = loc.Key };
                context.Departments.Add(department);
                await context.SaveChangesAsync();

                foreach (var cityName in loc.Value)
                {
                    context.Cities.Add(new City 
                    { 
                        Name = cityName, 
                        DepartmentId = department.Id 
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
