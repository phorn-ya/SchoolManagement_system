namespace SchoolManagementSystem.Models;

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Teacher";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ClassSection> TeachingSections { get; set; } = new List<ClassSection>();
}
