using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationIdToReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @db = DATABASE();
                SET @col_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'reviews' AND COLUMN_NAME = 'application_id');
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE `reviews` ADD `application_id` bigint NOT NULL DEFAULT 0',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("DELETE FROM reviews WHERE application_id = 0;");

            migrationBuilder.Sql("""
                SET @db = DATABASE();
                SET @idx_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'reviews' AND INDEX_NAME = 'IX_reviews_application_id_review_type');
                SET @sql = IF(@idx_exists = 0,
                    'CREATE UNIQUE INDEX `IX_reviews_application_id_review_type` ON `reviews` (`application_id`, `review_type`)',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @db = DATABASE();
                SET @fk_exists = (
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'reviews'
                      AND CONSTRAINT_NAME = 'FK_reviews_applications_application_id' AND CONSTRAINT_TYPE = 'FOREIGN KEY');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `reviews` ADD CONSTRAINT `FK_reviews_applications_application_id` FOREIGN KEY (`application_id`) REFERENCES `applications` (`id`) ON DELETE CASCADE',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reviews_applications_application_id",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_application_id_review_type",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "application_id",
                table: "reviews");
        }
    }
}
