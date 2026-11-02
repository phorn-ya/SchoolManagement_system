namespace SchoolManagementSystem.Models;

public class GradeAssessment
{
    public int Id { get; set; }
    public int ClassSectionId { get; set; }
    public ClassSection ClassSection { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public decimal PointsPossible { get; set; }
    public DateOnly? DueDate { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<StudentGrade> Grades { get; set; } = new List<StudentGrade>();
}
