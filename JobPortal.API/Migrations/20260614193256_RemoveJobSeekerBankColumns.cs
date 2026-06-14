using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobSeekerBankColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_number",
                table: "job_seekers");

            migrationBuilder.DropColumn(
                name: "bank_name",
                table: "job_seekers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_number",
                table: "job_seekers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                table: "job_seekers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
