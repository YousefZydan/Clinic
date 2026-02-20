using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeding
{
    public static class DefaultRolesAndUsersSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            // ✅ 1) Seed Roles
            var roles = new List<string> { "Admin", "Patient", "Doctor", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ✅ 2) Seed Default Admin
            var defaultUserEmail = "admin@gmail.com";
            var defaultUserName = "admin";

            var existingAdmin = await userManager.FindByEmailAsync(defaultUserEmail);

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = defaultUserName,
                    Email = defaultUserEmail,
                    Name = "System Admin",
                    Nickname = "Admin",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = Gender.Male,
                    PhotoUrl = "https://example.com/default-photo.png",
                    PhoneNumber = "+201000000000" // 📱 رقم الهاتف الافتراضي
                };

                var result = await userManager.CreateAsync(adminUser, "P@ssword123");

                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "User" });
                }
                else
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // ✅ 3) Seed Doctor Users
            var doctorUsers = new List<User>
            {
                new User { UserName = "dentist@demo.com", Email = "dentist@demo.com", Name = "Ahmed", Nickname = "Dentist", PhotoUrl = "default.jpg", Gender = Gender.Male, PhoneNumber = "+201000000001" },
                new User { UserName = "cardio@demo.com", Email = "cardio@demo.com", Name = "Ali", Nickname = "Cardio", PhotoUrl = "default.jpg", Gender = Gender.Male, PhoneNumber = "+201000000002" },
                new User { UserName = "pulmo@demo.com", Email = "pulmo@demo.com", Name = "Sara", Nickname = "Pulmo", PhotoUrl = "default.jpg", Gender = Gender.Female, PhoneNumber = "+201000000003" },
                new User { UserName = "general@demo.com", Email = "general@demo.com", Name = "Mona", Nickname = "General", PhotoUrl = "default.jpg", Gender = Gender.Female, PhoneNumber = "+201000000004" },
                new User { UserName = "neuro@demo.com", Email = "neuro@demo.com", Name = "Youssef", Nickname = "Neuro", PhotoUrl = "default.jpg", Gender = Gender.Male, PhoneNumber = "+201000000005" },
                new User { UserName = "gastro@demo.com", Email = "gastro@demo.com", Name = "Karim", Nickname = "Gastro", PhotoUrl = "default.jpg", Gender = Gender.Male, PhoneNumber = "+201000000006" },
                new User { UserName = "lab@demo.com", Email = "lab@demo.com", Name = "Laila", Nickname = "Lab", PhotoUrl = "default.jpg", Gender = Gender.Female, PhoneNumber = "+201000000007" },
                new User { UserName = "vaccine@demo.com", Email = "vaccine@demo.com", Name = "Hany", Nickname = "Vaccine", PhotoUrl = "default.jpg", Gender = Gender.Male, PhoneNumber = "+201000000008" }
            };

            foreach (var user in doctorUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(user, "Pa$$w0rd!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                    else
                    {
                        throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
