using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class ApplicationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            // ✅ Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category("Dentistry"),
                    new Category("Cardiology"),
                    new Category("Pulmonology"),
                    new Category("General"),
                    new Category("Neurology"),
                    new Category("Gastroenterology"),
                    new Category("Laboratory"),
                    new Category("Vaccinology")
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
            



            // ✅ Seed Doctors (ربط كل doctor بـ user و category)
            if (!context.Doctors.Any())
            {
                var categories = await context.Categories.ToListAsync();
                var users = await userManager.Users.ToListAsync();

                var doctors = categories.Select((cat, index) =>
                    new Doctor(
                        name: $"Dr. {users[index+1].Name}",
                        about: $"Experienced in {cat.Name} treatments.",
                        userId: users[index+1].Id,
                        categoryId: cat.Id
                    )).ToList();

                await context.Doctors.AddRangeAsync(doctors);
                await context.SaveChangesAsync();
            }
        }
    }
}
