using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations;

/// <inheritdoc />
public partial class ReplaceJobDatesWithWorkingHours : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "working_hours",
            table: "jobs",
            type: "varchar(50)",
            maxLength: 50,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.Sql(
            """
            UPDATE jobs
            SET working_hours = '8h-17h'
            WHERE working_hours IS NULL OR working_hours = '';
            """);

        migrationBuilder.DropColumn(
            name: "end_date",
            table: "jobs");

        migrationBuilder.DropColumn(
            name: "start_date",
            table: "jobs");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "end_date",
            table: "jobs",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc));

        migrationBuilder.AddColumn<DateTime>(
            name: "start_date",
            table: "jobs",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

        migrationBuilder.DropColumn(
            name: "working_hours",
            table: "jobs");
    }
}
