using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassSectionResponse>>> GetAll()
    {
        var query = db.ClassSections.AsNoTracking().AsQueryable();
        if (!User.IsInRole("Admin"))
        {
            var teacherId = ClassAccess.UserId(User);
            if (teacherId is null) return Forbid();
            query = query.Where(section => section.TeacherId == teacherId.Value);
        }

        var classes = await query.OrderBy(section => section.SectionCode)
            .Select(section => new ClassSectionResponse(
                section.Id, section.CourseId, section.Course.CourseCode, section.Course.Name,
                section.TeacherId, section.Teacher.FullName, section.SectionCode,
                section.Term, section.Schedule, section.Room, section.Enrollments.Count))
            .ToListAsync();
        return Ok(classes);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClassSectionResponse>> Create(ClassSectionCreateDto request)
    {
        var code = request.SectionCode.Trim().ToUpperInvariant();
        if (await db.ClassSections.AnyAsync(section => section.SectionCode == code))
            return Conflict(new { message = "Class section code already exists." });
        if (!await db.Courses.AnyAsync(course => course.Id == request.CourseId))
            return NotFound(new { message = "Course not found." });
        if (!await db.Users.AnyAsync(user => user.Id == request.TeacherId && user.Role == "Teacher"))
            return NotFound(new { message = "Teacher account not found." });

        var section = new ClassSection
        {
            CourseId = request.CourseId,
            TeacherId = request.TeacherId,
            SectionCode = code,
            Term = request.Term.Trim(),
            Schedule = request.Schedule?.Trim(),
            Room = request.Room?.Trim()
        };
        db.ClassSections.Add(section);
        await db.SaveChangesAsync();

        var response = await ToResponseAsync(section.Id);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ClassSectionUpdateDto request)
    {
        var section = await db.ClassSections.FindAsync(id);
        if (section is null) return NotFound();
        if (section.CourseId != request.CourseId &&
            await db.Enrollments.AnyAsync(enrollment => enrollment.ClassSectionId == id))
            return Conflict(new { message = "A class course cannot be changed after students are enrolled." });
        if (!await db.Courses.AnyAsync(course => course.Id == request.CourseId))
            return NotFound(new { message = "Course not found." });
        if (!await db.Users.AnyAsync(user => user.Id == request.TeacherId && user.Role == "Teacher"))
            return NotFound(new { message = "Teacher account not found." });

        section.CourseId = request.CourseId;
        section.TeacherId = request.TeacherId;
        section.Term = request.Term.Trim();
        section.Schedule = request.Schedule?.Trim();
        section.Room = request.Room?.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var section = await db.ClassSections
            .Include(item => item.Assessments)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (section is null) return NotFound();

        var attendance = await db.AttendanceRecords
            .Where(record => record.ClassSectionId == id).ToListAsync();
        db.AttendanceRecords.RemoveRange(attendance);
        db.ClassSections.Remove(section);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/roster")]
    public async Task<IActionResult> GetRoster(int id)
    {
        var section = await ClassAccess.FindAsync(db, User, id);
        if (section is null) return NotFound();

        var roster = await db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.ClassSectionId == id)
            .OrderBy(enrollment => enrollment.Student.LastName)
            .ThenBy(enrollment => enrollment.Student.FirstName)
            .Select(enrollment => new
            {
                enrollment.StudentId,
                enrollment.Student.StudentCode,
                StudentName = enrollment.Student.FirstName + " " + enrollment.Student.LastName,
                enrollment.EnrolledAtUtc
            }).ToListAsync();
        return Ok(roster);
    }

    [HttpPost("{id:int}/roster")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddToRoster(int id, ClassEnrollmentCreateDto request)
    {
        var section = await db.ClassSections.FindAsync(id);
        if (section is null) return NotFound(new { message = "Class not found." });
        if (!await db.Students.AnyAsync(student => student.Id == request.StudentId))
            return NotFound(new { message = "Student not found." });
        if (await db.Enrollments.AnyAsync(enrollment =>
            enrollment.ClassSectionId == id && enrollment.StudentId == request.StudentId))
            return Conflict(new { message = "Student is already enrolled in this class." });

        db.Enrollments.Add(new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = section.CourseId,
            ClassSectionId = id
        });
        await db.SaveChangesAsync();
        return Ok(new { request.StudentId, ClassSectionId = id });
    }

    [HttpDelete("{id:int}/roster/{studentId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveFromRoster(int id, int studentId)
    {
        var enrollment = await db.Enrollments.FirstOrDefaultAsync(item =>
            item.ClassSectionId == id && item.StudentId == studentId);
        if (enrollment is null) return NotFound();
        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Task<ClassSectionResponse?> ToResponseAsync(int id) =>
        db.ClassSections.AsNoTracking()
            .Where(section => section.Id == id)
            .Select(section => new ClassSectionResponse(
                section.Id, section.CourseId, section.Course.CourseCode, section.Course.Name,
                section.TeacherId, section.Teacher.FullName, section.SectionCode,
                section.Term, section.Schedule, section.Room, section.Enrollments.Count))
            .FirstOrDefaultAsync();
}
