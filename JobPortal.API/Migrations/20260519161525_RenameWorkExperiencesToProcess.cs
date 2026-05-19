using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations;

/// <inheritdoc />
public partial class RenameWorkExperiencesToProcess : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_work_experiences_applications_application_id",
            table: "work_experiences");

        migrationBuilder.RenameTable(
            name: "work_experiences",
            newName: "process");

        migrationBuilder.RenameIndex(
            name: "IX_work_experiences_application_id",
            newName: "IX_process_application_id",
            table: "process");

        migrationBuilder.AddForeignKey(
            name: "FK_process_applications_application_id",
            table: "process",
            column: "application_id",
            principalTable: "applications",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_process_applications_application_id",
            table: "process");

        migrationBuilder.RenameTable(
            name: "process",
            newName: "work_experiences");

        migrationBuilder.RenameIndex(
            name: "IX_process_application_id",
            newName: "IX_work_experiences_application_id",
            table: "work_experiences");

        migrationBuilder.AddForeignKey(
            name: "FK_work_experiences_applications_application_id",
            table: "work_experiences",
            column: "application_id",
            principalTable: "applications",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
