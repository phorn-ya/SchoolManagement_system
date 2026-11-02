using Microsoft.AspNetCore.Identity;
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
public class AuthController(
    ApplicationDbContext db,
    TokenService tokenService) : ControllerBase
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = "Teacher"
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new AuthResponse(tokenService.CreateToken(user), user.FullName, user.Email, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);

        if (user is null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new AuthResponse(tokenService.CreateToken(user), user.FullName, user.Email, user.Role));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> UpdateProfile(ProfileUpdateDto request)
    {
        var userId = ClassAccess.UserId(User);
        if (userId is null) return Unauthorized();
        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return Unauthorized();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(existing => existing.Id != user.Id && existing.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        user.FullName = request.FullName.Trim();
        user.Email = email;
        await db.SaveChangesAsync();
        return Ok(new AuthResponse(tokenService.CreateToken(user), user.FullName, user.Email, user.Role));
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(PasswordChangeDto request)
    {
        var userId = ClassAccess.UserId(User);
        if (userId is null) return Unauthorized();
        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return Unauthorized();

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword)
            == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
