using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Authorize]
public class GradesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("api/classes/{classId:int}/assessments")]
    public async Task<IActionResult> GetAssessments(int classId)
    {
        if (await ClassAccess.FindAsync(db, User, classId) is null) return NotFound();
        var assessments = await db.GradeAssessments.AsNoTracking()
            .Where(assessment => assessment.ClassSectionId == classId)
            .OrderBy(assessment => assessment.DueDate)
            .ThenBy(assessment => assessment.Title)
            .Select(assessment => new
            {
                assessment.Id,
                assessment.Title,
                assessment.PointsPossible,
                assessment.DueDate,
                Grades = assessment.ClassSection.Enrollments
                    .OrderBy(enrollment => enrollment.Student.LastName)
                    .ThenBy(enrollment => enrollment.Student.FirstName)
                    .Select(enrollment => new
                    {
                        enrollment.StudentId,
                        enrollment.Student.StudentCode,
                        StudentName = enrollment.Student.FirstName + " " + enrollment.Student.LastName,
                        Score = assessment.Grades
                            .Where(grade => grade.StudentId == enrollment.StudentId)
                            .Select(grade => grade.Score)
                            .FirstOrDefault()
                    }).ToList()
            }).ToListAsync();

        return Ok(assessments.Select(assessment => new AssessmentResponse(
            assessment.Id,
            assessment.Title,
            assessment.PointsPossible,
            assessment.DueDate,
            assessment.Grades.Select(grade =>
            {
                var percentage = grade.Score is null
                    ? (decimal?)null
                    : Math.Round(grade.Score.Value / assessment.PointsPossible * 100, 2);
                return new GradeResponse(
                    grade.StudentId, grade.StudentCode, grade.StudentName, grade.Score,
                    assessment.PointsPossible, percentage, LetterGrade(percentage));
            }).ToList())));
    }

    [HttpPost("api/classes/{classId:int}/assessments")]
    public async Task<IActionResult> CreateAssessment(int classId, AssessmentCreateDto request)
    {
        var section = await ClassAccess.FindAsync(db, User, classId);
        if (section is null) return NotFound();
        if (request.PointsPossible <= 0)
            return BadRequest(new { message = "Points possible must be greater than zero." });

        var assessment = new GradeAssessment
        {
            ClassSectionId = classId,
            Title = request.Title.Trim(),
            PointsPossible = request.PointsPossible,
            DueDate = request.DueDate,
            CreatedByUserId = ClassAccess.UserId(User)!.Value
        };
        db.GradeAssessments.Add(assessment);
        await db.SaveChangesAsync();

        var enrolledIds = await db.Enrollments
            .Where(enrollment => enrollment.ClassSectionId == classId)
            .Select(enrollment => enrollment.StudentId).ToListAsync();
        db.StudentGrades.AddRange(enrolledIds.Select(studentId => new StudentGrade
        {
            AssessmentId = assessment.Id,
            StudentId = studentId
        }));
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAssessments), new { classId }, new { assessment.Id, assessment.Title });
    }

    [HttpPut("api/assessments/{assessmentId:int}/grades")]
    public async Task<IActionResult> SaveGrades(int assessmentId, GradeBatchDto request)
    {
        var assessment = await db.GradeAssessments
            .Include(item => item.ClassSection)
            .FirstOrDefaultAsync(item => item.Id == assessmentId);
        if (assessment is null) return NotFound();
        if (await ClassAccess.FindAsync(db, User, assessment.ClassSectionId) is null) return NotFound();

        var entries = request.Grades;
        if (entries.Select(entry => entry.StudentId).Distinct().Count() != entries.Count)
            return BadRequest(new { message = "A student can appear only once in a grades submission." });
        if (entries.Any(entry => entry.Score > assessment.PointsPossible))
            return BadRequest(new { message = "A score cannot exceed the assessment's points possible." });

        var studentIds = entries.Select(entry => entry.StudentId).ToArray();
        var enrolledIds = await db.Enrollments
            .Where(enrollment => enrollment.ClassSectionId == assessment.ClassSectionId &&
                                 studentIds.Contains(enrollment.StudentId))
            .Select(enrollment => enrollment.StudentId).ToListAsync();
        if (enrolledIds.Count != studentIds.Length)
            return BadRequest(new { message = "Grades can only be recorded for students enrolled in this class." });

        var grades = await db.StudentGrades
            .Where(grade => grade.AssessmentId == assessmentId && studentIds.Contains(grade.StudentId))
            .ToDictionaryAsync(grade => grade.StudentId);
        foreach (var entry in entries)
        {
            if (!grades.TryGetValue(entry.StudentId, out var grade))
            {
                grade = new StudentGrade { AssessmentId = assessmentId, StudentId = entry.StudentId };
                db.StudentGrades.Add(grade);
            }
            grade.Score = entry.Score;
            grade.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/assessments/{assessmentId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAssessment(int assessmentId)
    {
        var assessment = await db.GradeAssessments.FindAsync(assessmentId);
        if (assessment is null) return NotFound();
        db.GradeAssessments.Remove(assessment);
        await db.SaveChangesAsync();
        return NoContent();
    }

    public static string? LetterGrade(decimal? percentage) => percentage switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        not null => "F",
        _ => null
    };
}
