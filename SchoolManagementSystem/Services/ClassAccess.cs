using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Services;

public static class ClassAccess
{
    public static int? UserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static Task<ClassSection?> FindAsync(
        ApplicationDbContext db,
        ClaimsPrincipal user,
        int classId)
    {
        var query = db.ClassSections.AsQueryable();
        if (!user.IsInRole("Admin"))
        {
            var userId = UserId(user);
            if (userId is null) return Task.FromResult<ClassSection?>(null);
            query = query.Where(section => section.TeacherId == userId.Value);
        }

        return query.FirstOrDefaultAsync(section => section.Id == classId);
    }
}
