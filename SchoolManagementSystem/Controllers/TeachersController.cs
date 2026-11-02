using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeachersController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAll()
    {
        var teachers = await db.Users.AsNoTracking()
            .Where(user => user.Role == "Teacher")
            .OrderBy(user => user.FullName)
            .Select(user => new TeacherResponse(user.Id, user.FullName, user.Email, user.CreatedAtUtc))
            .ToListAsync();
        return Ok(teachers);
    }

    [HttpPost]
    public async Task<ActionResult<TeacherResponse>> Create(TeacherCreateDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var teacher = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = "Teacher"
        };
        teacher.PasswordHash = new PasswordHasher<AppUser>()
            .HashPassword(teacher, request.TemporaryPassword);

        db.Users.Add(teacher);
        await db.SaveChangesAsync();

        var response = new TeacherResponse(teacher.Id, teacher.FullName, teacher.Email, teacher.CreatedAtUtc);
        return Ok(response);
    }
}
