using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await db.Enrollments.AsNoTracking()
            .Include(x => x.Student).Include(x => x.Course)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                StudentName = x.Student.FirstName + " " + x.Student.LastName,
                x.CourseId,
                CourseName = x.Course.Name,
                x.EnrolledAtUtc,
                x.Status
            }).ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EnrollmentCreateDto request)
    {
        if (!await db.Students.AnyAsync(x => x.Id == request.StudentId))
            return NotFound(new { message = "Student not found." });

        if (!await db.Courses.AnyAsync(x => x.Id == request.CourseId))
            return NotFound(new { message = "Course not found." });

        if (await db.Enrollments.AnyAsync(x =>
            x.StudentId == request.StudentId && x.CourseId == request.CourseId))
            return Conflict(new { message = "Student is already enrolled in this course." });

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();
        return Ok(new { enrollment.Id, enrollment.StudentId, enrollment.CourseId, enrollment.Status });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var enrollment = await db.Enrollments.FindAsync(id);
        if (enrollment is null) return NotFound();

        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
