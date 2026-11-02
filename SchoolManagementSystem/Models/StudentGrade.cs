namespace SchoolManagementSystem.Models;

public class StudentGrade
{
    public int Id { get; set; }
    public int AssessmentId { get; set; }
    public GradeAssessment Assessment { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public decimal? Score { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
