using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.DTOs;

public record CourseCreateDto(
    [Required, MaxLength(30)] string CourseCode,
    [Required, MaxLength(150)] string Name,
    string? Description,
    [Range(1, 30)] int Credits);

public record CourseUpdateDto(
    [Required, MaxLength(150)] string Name,
    string? Description,
    [Range(1, 30)] int Credits);

public record CourseResponse(int Id, string CourseCode, string Name, string? Description, int Credits);
public record EnrollmentCreateDto([Range(1, int.MaxValue)] int StudentId, [Range(1, int.MaxValue)] int CourseId);
