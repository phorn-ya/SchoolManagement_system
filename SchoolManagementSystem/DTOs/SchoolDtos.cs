using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.DTOs;

public record TeacherCreateDto(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8)] string TemporaryPassword);

public record TeacherResponse(int Id, string FullName, string Email, DateTime CreatedAtUtc);

public record ClassSectionCreateDto(
    [Range(1, int.MaxValue)] int CourseId,
    [Range(1, int.MaxValue)] int TeacherId,
    [Required, MaxLength(30)] string SectionCode,
    [Required, MaxLength(100)] string Term,
    [MaxLength(200)] string? Schedule,
    [MaxLength(100)] string? Room);

public record ClassSectionUpdateDto(
    [Range(1, int.MaxValue)] int CourseId,
    [Range(1, int.MaxValue)] int TeacherId,
    [Required, MaxLength(100)] string Term,
    [MaxLength(200)] string? Schedule,
    [MaxLength(100)] string? Room);

public record ClassSectionResponse(
    int Id, int CourseId, string CourseCode, string CourseName, int TeacherId,
    string TeacherName, string SectionCode, string Term, string? Schedule,
    string? Room, int StudentCount);

public record ClassEnrollmentCreateDto([Range(1, int.MaxValue)] int StudentId);

public record AttendanceEntryDto(
    [Range(1, int.MaxValue)] int StudentId,
    [Required, RegularExpression("Present|Absent|Late|Excused")] string Status,
    [MaxLength(500)] string? Notes);

public record AttendanceBatchDto(
    DateOnly AttendanceDate,
    [Required, MinLength(1)] List<AttendanceEntryDto> Records);

public record AttendanceResponse(
    int StudentId, string StudentCode, string StudentName, DateOnly AttendanceDate,
    string? Status, string? Notes);

public record AssessmentCreateDto(
    [Required, MaxLength(150)] string Title,
    [Range(typeof(decimal), "0.01", "1000000")] decimal PointsPossible,
    DateOnly? DueDate);

public record GradeEntryDto(
    [Range(1, int.MaxValue)] int StudentId,
    [Range(typeof(decimal), "0", "1000000")] decimal? Score);

public record GradeBatchDto([Required, MinLength(1)] List<GradeEntryDto> Grades);

public record GradeResponse(
    int StudentId, string StudentCode, string StudentName, decimal? Score,
    decimal PointsPossible, decimal? Percentage, string? LetterGrade);

public record AssessmentResponse(
    int Id, string Title, decimal PointsPossible, DateOnly? DueDate,
    IReadOnlyList<GradeResponse> Grades);

public record SchoolSettingsDto(
    [Required, MaxLength(150)] string SchoolName,
    [Required, MaxLength(100)] string TimeZone);

public record ProfileUpdateDto(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email);

public record PasswordChangeDto(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword);
