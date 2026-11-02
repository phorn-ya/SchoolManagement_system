namespace SchoolManagementSystem.Models;

public class ClassSection
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int TeacherId { get; set; }
    public AppUser Teacher { get; set; } = null!;
    public string SectionCode { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string? Schedule { get; set; }
    public string? Room { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<GradeAssessment> Assessments { get; set; } = new List<GradeAssessment>();
}
