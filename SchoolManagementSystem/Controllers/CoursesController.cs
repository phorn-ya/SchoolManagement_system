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
public class CoursesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
    {
        var courses = await db.Courses.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CourseResponse(x.Id, x.CourseCode, x.Name, x.Description, x.Credits))
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseResponse>> GetById(int id)
    {
        var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (course is null) return NotFound();
        return Ok(new CourseResponse(course.Id, course.CourseCode, course.Name, course.Description, course.Credits));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseResponse>> Create(CourseCreateDto request)
    {
        var code = request.CourseCode.Trim().ToUpperInvariant();
        if (await db.Courses.AnyAsync(x => x.CourseCode == code))
            return Conflict(new { message = "Course code already exists." });

        var course = new Course
        {
            CourseCode = code,
            Name = request.Name.Trim(),
            Description = request.Description,
            Credits = request.Credits
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var response = new CourseResponse(course.Id, course.CourseCode, course.Name, course.Description, course.Credits);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CourseUpdateDto request)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null) return NotFound();

        course.Name = request.Name.Trim();
        course.Description = request.Description;
        course.Credits = request.Credits;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null) return NotFound();
        if (await db.ClassSections.AnyAsync(section => section.CourseId == id))
            return Conflict(new { message = "Delete this course's class sections before deleting the course." });

        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
