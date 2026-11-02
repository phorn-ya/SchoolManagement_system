using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.DTOs;

public record StudentCreateDto(
    [Required, MaxLength(30)] string StudentCode,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    DateOnly DateOfBirth,
    [Required, MaxLength(20)] string Gender,
    [Required, EmailAddress, MaxLength(200)] string Email,
    string? Phone,
    string? Address);

public record StudentUpdateDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    DateOnly DateOfBirth,
    [Required, MaxLength(20)] string Gender,
    [Required, EmailAddress, MaxLength(200)] string Email,
    string? Phone,
    string? Address);

public record StudentResponse(
    int Id, string StudentCode, string FirstName, string LastName,
    DateOnly DateOfBirth, string Gender, string Email, string? Phone, string? Address);
