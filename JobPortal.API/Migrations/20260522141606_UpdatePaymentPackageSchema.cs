using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentPackageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "posting_packages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "posting_packages",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "posting_packages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "posting_packages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "payment_histories",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "payment_histories",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "VND")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "expired_at",
                table: "payment_histories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "package_name_snapshot",
                table: "payment_histories",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_provider",
                table: "payment_histories",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "posting_limit_snapshot",
                table: "payment_histories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "price_snapshot",
                table: "payment_histories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_id",
                table: "payment_histories",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "payment_histories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_posting_packages_name",
                table: "posting_packages",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_histories_order_id",
                table: "payment_histories",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_histories_provider_transaction_id",
                table: "payment_histories",
                column: "provider_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_histories_transaction_code",
                table: "payment_histories",
                column: "transaction_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_posting_packages_name",
                table: "posting_packages");

            migrationBuilder.DropIndex(
                name: "IX_payment_histories_order_id",
                table: "payment_histories");

            migrationBuilder.DropIndex(
                name: "IX_payment_histories_provider_transaction_id",
                table: "payment_histories");

            migrationBuilder.DropIndex(
                name: "IX_payment_histories_transaction_code",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "posting_packages");

            migrationBuilder.DropColumn(
                name: "description",
                table: "posting_packages");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "posting_packages");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "posting_packages");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "expired_at",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "package_name_snapshot",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "payment_provider",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "posting_limit_snapshot",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "price_snapshot",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "provider_transaction_id",
                table: "payment_histories");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "payment_histories");
        }
    }
}
