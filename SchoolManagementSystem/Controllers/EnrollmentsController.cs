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
                ClassSectionCode = x.ClassSection == null ? null : x.ClassSection.SectionCode,
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

        var section = request.ClassSectionId is int classSectionId
            ? await db.ClassSections.FirstOrDefaultAsync(x => x.Id == classSectionId)
            : null;
        if (request.ClassSectionId is not null && section is null)
            return NotFound(new { message = "Class not found." });

        if (section is not null && section.CourseId != request.CourseId)
            return BadRequest(new { message = "The selected class does not belong to this course." });

        if (!await db.Courses.AnyAsync(x => x.Id == request.CourseId))
            return NotFound(new { message = "Course not found." });

        var classSectionIdForEnrollment = section?.Id;
        if (section is null)
        {
            var matchingSections = await db.ClassSections
                .Where(x => x.CourseId == request.CourseId)
                .Select(x => x.Id)
                .Take(2)
                .ToListAsync();
            if (matchingSections.Count == 1)
                classSectionIdForEnrollment = matchingSections[0];
        }

        var existingEnrollment = await db.Enrollments.FirstOrDefaultAsync(x =>
            x.StudentId == request.StudentId && x.CourseId == request.CourseId);
        if (existingEnrollment is not null)
        {
            if (classSectionIdForEnrollment is not null && existingEnrollment.ClassSectionId is null)
            {
                existingEnrollment.ClassSectionId = classSectionIdForEnrollment;
                await db.SaveChangesAsync();
                return Ok(new
                {
                    existingEnrollment.Id,
                    existingEnrollment.StudentId,
                    existingEnrollment.CourseId,
                    existingEnrollment.ClassSectionId,
                    existingEnrollment.Status
                });
            }

            return Conflict(new { message = "Student is already enrolled in this course." });
        }

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            ClassSectionId = classSectionIdForEnrollment
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();
        return Ok(new
        {
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.ClassSectionId,
            enrollment.Status
        });
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
