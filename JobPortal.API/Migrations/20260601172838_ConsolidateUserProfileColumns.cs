using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateUserProfileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "profile_image",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email",
                table: "job_seekers");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "job_seekers");

            migrationBuilder.DropColumn(
                name: "role",
                table: "job_seekers");

            migrationBuilder.DropColumn(
                name: "email",
                table: "employers");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "employers");

            migrationBuilder.DropColumn(
                name: "role",
                table: "employers");

            migrationBuilder.DropColumn(
                name: "email",
                table: "admins");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "admins");

            migrationBuilder.DropColumn(
                name: "role",
                table: "admins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "profile_image",
                table: "users",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "job_seekers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "job_seekers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "job_seekers",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "employers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "employers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "employers",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "admins",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "admins",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "admins",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
