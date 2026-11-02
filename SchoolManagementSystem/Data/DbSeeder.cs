using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            var admin = new AppUser
            {
                FullName = "System Administrator",
                Email = "admin@school.local",
                Role = "Admin"
            };

            admin.PasswordHash = new PasswordHasher<AppUser>()
                .HashPassword(admin, "ChangeMe123!");

            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        if (!await db.Courses.AnyAsync())
        {
            db.Courses.AddRange(
                new Course { CourseCode = "MATH-101", Name = "Mathematics", Description = "Foundational mathematics", Credits = 3 },
                new Course { CourseCode = "ENG-101", Name = "English", Description = "English language", Credits = 3 }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.SchoolSettings.AnyAsync())
        {
            db.SchoolSettings.Add(new SchoolSettings());
            await db.SaveChangesAsync();
        }
    }
}
