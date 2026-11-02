using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/classes/{classId:int}/attendance")]
[Authorize]
public class AttendanceController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForClass(int classId, [FromQuery] DateOnly date)
    {
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();

        var roster = await db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.ClassSectionId == classId)
            .OrderBy(enrollment => enrollment.Student.LastName)
            .ThenBy(enrollment => enrollment.Student.FirstName)
            .Select(enrollment => new
            {
                enrollment.StudentId,
                enrollment.Student.StudentCode,
                StudentName = enrollment.Student.FirstName + " " + enrollment.Student.LastName
            }).ToListAsync();

        var records = await db.AttendanceRecords.AsNoTracking()
            .Where(record => record.ClassSectionId == classId && record.AttendanceDate == date)
            .ToDictionaryAsync(record => record.StudentId);

        return Ok(roster.Select(student => new AttendanceResponse(
            student.StudentId,
            student.StudentCode,
            student.StudentName,
            date,
            records.TryGetValue(student.StudentId, out var record) ? record.Status : null,
            records.TryGetValue(student.StudentId, out record) ? record.Notes : null)));
    }

    [HttpPut]
    public async Task<IActionResult> SaveForClass(int classId, AttendanceBatchDto request)
    {
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();
        var studentIds = request.Records.Select(item => item.StudentId).ToArray();
        if (studentIds.Distinct().Count() != studentIds.Length)
            return BadRequest(new { message = "A student can appear only once in an attendance submission." });

        var enrolledIds = await db.Enrollments
            .Where(enrollment => enrollment.ClassSectionId == classId && studentIds.Contains(enrollment.StudentId))
            .Select(enrollment => enrollment.StudentId).ToListAsync();
        if (enrolledIds.Count != studentIds.Length)
            return BadRequest(new { message = "Attendance can only be recorded for students enrolled in this class." });

        var records = await db.AttendanceRecords
            .Where(record => record.ClassSectionId == classId && record.AttendanceDate == request.AttendanceDate)
            .ToDictionaryAsync(record => record.StudentId);
        var userId = ClassAccess.UserId(User);
        if (userId is null) return Unauthorized();

        foreach (var entry in request.Records)
        {
            if (!records.TryGetValue(entry.StudentId, out var record))
            {
                record = new AttendanceRecord
                {
                    ClassSectionId = classId,
                    StudentId = entry.StudentId,
                    AttendanceDate = request.AttendanceDate,
                    RecordedByUserId = userId.Value
                };
                db.AttendanceRecords.Add(record);
            }

            record.Status = entry.Status;
            record.Notes = entry.Notes?.Trim();
            record.RecordedByUserId = userId.Value;
            record.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }
}
