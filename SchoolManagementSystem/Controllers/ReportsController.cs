using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Services;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("attendance-summary")]
    public async Task<IActionResult> AttendanceSummary([FromQuery] DateOnly date)
    {
        var records = db.AttendanceRecords.AsNoTracking()
            .Where(item => item.AttendanceDate == date);
        if (!User.IsInRole("Admin"))
        {
            var teacherId = ClassAccess.UserId(User);
            if (teacherId is null) return Unauthorized();
            records = records.Where(item => item.ClassSection.TeacherId == teacherId.Value);
        }

        var statuses = await records.Select(item => item.Status).ToListAsync();
        return Ok(new
        {
            TotalRecords = statuses.Count,
            PresentCount = statuses.Count(status => status is "Present" or "Late"),
            AbsentCount = statuses.Count(status => status == "Absent"),
            ExcusedCount = statuses.Count(status => status == "Excused")
        });
    }

    [HttpGet("roster")]
    public async Task<IActionResult> Roster([FromQuery] int classId)
    {
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();
        var roster = await db.Enrollments.AsNoTracking()
            .Where(item => item.ClassSectionId == classId)
            .OrderBy(item => item.Student.LastName).ThenBy(item => item.Student.FirstName)
            .Select(item => new
            {
                item.Student.StudentCode,
                item.Student.FirstName,
                item.Student.LastName,
                item.Student.Email,
                item.Student.Phone,
                item.EnrolledAtUtc
            }).ToListAsync();
        return Ok(roster);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance(
        [FromQuery] int classId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        if (to < from) return BadRequest(new { message = "The end date must be on or after the start date." });
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();

        var records = await db.AttendanceRecords.AsNoTracking()
            .Where(item => item.ClassSectionId == classId &&
                           item.AttendanceDate >= from && item.AttendanceDate <= to)
            .OrderBy(item => item.AttendanceDate).ThenBy(item => item.Student.LastName)
            .Select(item => new
            {
                item.AttendanceDate,
                item.Student.StudentCode,
                StudentName = item.Student.FirstName + " " + item.Student.LastName,
                item.Status,
                item.Notes
            }).ToListAsync();
        return Ok(records);
    }

    [HttpGet("grades")]
    public async Task<IActionResult> Grades([FromQuery] int classId)
    {
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();
        var assessments = await db.GradeAssessments.AsNoTracking()
            .Where(item => item.ClassSectionId == classId)
            .OrderBy(item => item.Title)
            .SelectMany(item => item.ClassSection.Enrollments.Select(enrollment => new
            {
                Assessment = item.Title,
                item.PointsPossible,
                enrollment.Student.StudentCode,
                StudentName = enrollment.Student.FirstName + " " + enrollment.Student.LastName,
                Score = item.Grades.Where(grade => grade.StudentId == enrollment.StudentId)
                    .Select(grade => grade.Score).FirstOrDefault()
            })).ToListAsync();
        return Ok(assessments.Select(assessment => new
        {
            assessment.StudentCode,
            assessment.StudentName,
            assessment.Assessment,
            assessment.Score,
            assessment.PointsPossible,
            Percentage = assessment.Score is null
                ? (decimal?)null
                : Math.Round(assessment.Score.Value / assessment.PointsPossible * 100, 2),
            LetterGrade = GradesController.LetterGrade(assessment.Score is null
                ? null
                : Math.Round(assessment.Score.Value / assessment.PointsPossible * 100, 2))
        }));
    }
}
