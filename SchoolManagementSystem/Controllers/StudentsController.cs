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
public class StudentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponse>>> GetAll()
    {
        var students = await db.Students.AsNoTracking()
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => ToResponse(x)).ToListAsync();
        return Ok(students);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return student is null ? NotFound() : Ok(ToResponse(student));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StudentResponse>> Create(StudentCreateDto request)
    {
        var code = request.StudentCode.Trim().ToUpperInvariant();
        if (await db.Students.AnyAsync(x => x.StudentCode == code))
            return Conflict(new { message = "Student code already exists." });

        var student = new Student
        {
            StudentCode = code,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone,
            Address = request.Address
        };

        db.Students.Add(student);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, ToResponse(student));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, StudentUpdateDto request)
    {
        var student = await db.Students.FindAsync(id);
        if (student is null) return NotFound();

        student.FirstName = request.FirstName.Trim();
        student.LastName = request.LastName.Trim();
        student.DateOfBirth = request.DateOfBirth;
        student.Gender = request.Gender.Trim();
        student.Email = request.Email.Trim().ToLowerInvariant();
        student.Phone = request.Phone;
        student.Address = request.Address;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await db.Students.FindAsync(id);
        if (student is null) return NotFound();

        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static StudentResponse ToResponse(Student x) =>
        new(x.Id, x.StudentCode, x.FirstName, x.LastName, x.DateOfBirth,
            x.Gender, x.Email, x.Phone, x.Address);
}
