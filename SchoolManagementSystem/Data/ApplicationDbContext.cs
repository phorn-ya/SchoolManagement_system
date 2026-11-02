using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<GradeAssessment> GradeAssessments => Set<GradeAssessment>();
    public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();
    public DbSet<SchoolSettings> SchoolSettings => Set<SchoolSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<AppUser>()
            .Property(x => x.Email).HasMaxLength(200);
        modelBuilder.Entity<AppUser>()
            .Property(x => x.Role).HasMaxLength(30);

        modelBuilder.Entity<Student>()
            .HasIndex(x => x.StudentCode).IsUnique();
        modelBuilder.Entity<Student>()
            .Property(x => x.StudentCode).HasMaxLength(30);
        modelBuilder.Entity<Student>()
            .Property(x => x.FirstName).HasMaxLength(100);
        modelBuilder.Entity<Student>()
            .Property(x => x.LastName).HasMaxLength(100);
        modelBuilder.Entity<Student>()
            .Property(x => x.Email).HasMaxLength(200);

        modelBuilder.Entity<Course>()
            .HasIndex(x => x.CourseCode).IsUnique();
        modelBuilder.Entity<Course>()
            .Property(x => x.CourseCode).HasMaxLength(30);
        modelBuilder.Entity<Course>()
            .Property(x => x.Name).HasMaxLength(150);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(x => new { x.StudentId, x.ClassSectionId })
            .IsUnique()
            .HasFilter("[ClassSectionId] IS NOT NULL");

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Student)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Course)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClassSection>()
            .HasIndex(x => x.SectionCode).IsUnique();
        modelBuilder.Entity<ClassSection>()
            .Property(x => x.SectionCode).HasMaxLength(30);
        modelBuilder.Entity<ClassSection>()
            .Property(x => x.Term).HasMaxLength(100);
        modelBuilder.Entity<ClassSection>()
            .Property(x => x.Schedule).HasMaxLength(200);
        modelBuilder.Entity<ClassSection>()
            .Property(x => x.Room).HasMaxLength(100);
        modelBuilder.Entity<ClassSection>()
            .HasOne(x => x.Course)
            .WithMany(x => x.ClassSections)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ClassSection>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.TeachingSections)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.ClassSection)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.ClassSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(x => new { x.ClassSectionId, x.StudentId, x.AttendanceDate })
            .IsUnique();
        modelBuilder.Entity<AttendanceRecord>()
            .Property(x => x.Status).HasMaxLength(20);
        modelBuilder.Entity<AttendanceRecord>()
            .Property(x => x.Notes).HasMaxLength(500);
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.ClassSection)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.ClassSectionId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GradeAssessment>()
            .Property(x => x.Title).HasMaxLength(150);
        modelBuilder.Entity<GradeAssessment>()
            .Property(x => x.PointsPossible).HasPrecision(10, 2);
        modelBuilder.Entity<GradeAssessment>()
            .HasOne(x => x.ClassSection)
            .WithMany(x => x.Assessments)
            .HasForeignKey(x => x.ClassSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentGrade>()
            .HasIndex(x => new { x.AssessmentId, x.StudentId }).IsUnique();
        modelBuilder.Entity<StudentGrade>()
            .Property(x => x.Score).HasPrecision(10, 2);
        modelBuilder.Entity<StudentGrade>()
            .HasOne(x => x.Assessment)
            .WithMany(x => x.Grades)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StudentGrade>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SchoolSettings>()
            .Property(x => x.SchoolName).HasMaxLength(150);
        modelBuilder.Entity<SchoolSettings>()
            .Property(x => x.TimeZone).HasMaxLength(100);
    }
}
