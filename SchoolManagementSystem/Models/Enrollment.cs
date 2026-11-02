namespace SchoolManagementSystem.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int? ClassSectionId { get; set; }
    public ClassSection? ClassSection { get; set; }
    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active";
}
