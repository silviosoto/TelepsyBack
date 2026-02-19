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

                // Create Person for Admin
                var person = new Person
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
                context.People.Add(person);
                await context.SaveChangesAsync();

                var admin = new Admin
                {
                    PersonId = person.Id,
                    IsActive = true,
                    Department = "IT Support"
                };
                context.Admins.Add(admin);
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
        }
    }
}
