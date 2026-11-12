using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SchoolManagementSystem.Data;

#nullable disable

namespace SchoolManagementSystem.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261112090000_LinkUnassignedEnrollmentsToSingleClassSection")]
public partial class LinkUnassignedEnrollmentsToSingleClassSection : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [enrollment]
            SET [ClassSectionId] = [section].[Id]
            FROM [Enrollments] AS [enrollment]
            INNER JOIN (
                SELECT [CourseId], MIN([Id]) AS [Id]
                FROM [ClassSections]
                GROUP BY [CourseId]
                HAVING COUNT(*) = 1
            ) AS [section] ON [enrollment].[CourseId] = [section].[CourseId]
            WHERE [enrollment].[ClassSectionId] IS NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM [Enrollments] AS [assigned]
                  WHERE [assigned].[StudentId] = [enrollment].[StudentId]
                    AND [assigned].[ClassSectionId] = [section].[Id]
              )
              AND [enrollment].[Id] = (
                  SELECT MIN([candidate].[Id])
                  FROM [Enrollments] AS [candidate]
                  WHERE [candidate].[StudentId] = [enrollment].[StudentId]
                    AND [candidate].[CourseId] = [enrollment].[CourseId]
                    AND [candidate].[ClassSectionId] IS NULL
              );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
