using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.DTOs;

namespace SchoolManagementSystem.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SchoolSettingsDto>> Get()
    {
        var settings = await db.SchoolSettings.AsNoTracking().FirstOrDefaultAsync(item => item.Id == 1);
        if (settings is null) return NotFound(new { message = "School settings have not been initialized." });
        return Ok(new SchoolSettingsDto(settings.SchoolName, settings.TimeZone));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(SchoolSettingsDto request)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return BadRequest(new { message = "Select a valid time zone." });
        }
        catch (InvalidTimeZoneException)
        {
            return BadRequest(new { message = "Select a valid time zone." });
        }

        var settings = await db.SchoolSettings.FindAsync(1);
        if (settings is null) return NotFound(new { message = "School settings have not been initialized." });
        settings.SchoolName = request.SchoolName.Trim();
        settings.TimeZone = request.TimeZone;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
