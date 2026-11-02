namespace SchoolManagementSystem.Models;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int ClassSectionId { get; set; }
    public ClassSection ClassSection { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public DateOnly AttendanceDate { get; set; }
    public string Status { get; set; } = "Present";
    public string? Notes { get; set; }
    public int RecordedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
