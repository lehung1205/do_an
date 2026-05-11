using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class ErdsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.CreateTable(
                name: "danh_muc",
                columns: table => new
                {
                    id_danh_muc = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danh_muc", x => x.id_danh_muc);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nguoi_tim_viec",
                columns: table => new
                {
                    id_tim_viec = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_xac_thuc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    mat_khau = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sdt = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    gioi_tinh = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    mo_ta = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anh_dai_dien = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cccd = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_cap_cccd = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_cap = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_chi_thuong_tru = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_chi_tam_tru = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tai_khoan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_ngan_hang = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguoi_tim_viec", x => x.id_tim_viec);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nha_tuyen_dung",
                columns: table => new
                {
                    id_tuyen_dung = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_xac_thuc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    mat_khau = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_sinh = table.Column<DateOnly>(type: "date", nullable: true),
                    gioi_tinh = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    mo_ta = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anh = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_luot_bai_dang = table.Column<int>(type: "int", nullable: false),
                    cccd = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sdt = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nha_tuyen_dung", x => x.id_tuyen_dung);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quan_tri_vien",
                columns: table => new
                {
                    id_quan_tri = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mat_khau = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_tai_khoan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_ngan_hang = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quan_tri_vien", x => x.id_quan_tri);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ho_so_tuyen_dung",
                columns: table => new
                {
                    id_cv = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_tim_viec = table.Column<long>(type: "bigint", nullable: false),
                    tieu_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ho_so_tuyen_dung", x => x.id_cv);
                    table.ForeignKey(
                        name: "FK_ho_so_tuyen_dung_nguoi_tim_viec_id_tim_viec",
                        column: x => x.id_tim_viec,
                        principalTable: "nguoi_tim_viec",
                        principalColumn: "id_tim_viec",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cong_viec",
                columns: table => new
                {
                    id_cong_viec = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_tuyen_dung = table.Column<long>(type: "bigint", nullable: false),
                    id_danh_muc = table.Column<long>(type: "bigint", nullable: false),
                    tieu_de = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_luong = table.Column<int>(type: "int", nullable: false),
                    dia_diem = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_bai_dang = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_bat_dau = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_ket_thuc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_het_han = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cong_viec", x => x.id_cong_viec);
                    table.ForeignKey(
                        name: "FK_cong_viec_danh_muc_id_danh_muc",
                        column: x => x.id_danh_muc,
                        principalTable: "danh_muc",
                        principalColumn: "id_danh_muc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cong_viec_nha_tuyen_dung_id_tuyen_dung",
                        column: x => x.id_tuyen_dung,
                        principalTable: "nha_tuyen_dung",
                        principalColumn: "id_tuyen_dung",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goi_bai_dang",
                columns: table => new
                {
                    id_goi = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_quan_tri = table.Column<long>(type: "bigint", nullable: false),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gia = table.Column<int>(type: "int", nullable: false),
                    so_luot_dang_bai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goi_bai_dang", x => x.id_goi);
                    table.ForeignKey(
                        name: "FK_goi_bai_dang_quan_tri_vien_id_quan_tri",
                        column: x => x.id_quan_tri,
                        principalTable: "quan_tri_vien",
                        principalColumn: "id_quan_tri",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "danh_gia",
                columns: table => new
                {
                    id_danh_gia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cong_viec = table.Column<long>(type: "bigint", nullable: false),
                    id_tuyen_dung = table.Column<long>(type: "bigint", nullable: false),
                    id_tim_viec = table.Column<long>(type: "bigint", nullable: false),
                    binh_luan = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    danh_gia = table.Column<int>(type: "int", nullable: false),
                    danh_gia_toi = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danh_gia", x => x.id_danh_gia);
                    table.ForeignKey(
                        name: "FK_danh_gia_cong_viec_id_cong_viec",
                        column: x => x.id_cong_viec,
                        principalTable: "cong_viec",
                        principalColumn: "id_cong_viec",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_danh_gia_nguoi_tim_viec_id_tim_viec",
                        column: x => x.id_tim_viec,
                        principalTable: "nguoi_tim_viec",
                        principalColumn: "id_tim_viec",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_danh_gia_nha_tuyen_dung_id_tuyen_dung",
                        column: x => x.id_tuyen_dung,
                        principalTable: "nha_tuyen_dung",
                        principalColumn: "id_tuyen_dung",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hinh_anh",
                columns: table => new
                {
                    id_hinh_anh = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cong_viec = table.Column<long>(type: "bigint", nullable: false),
                    duong_dan_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hinh_anh", x => x.id_hinh_anh);
                    table.ForeignKey(
                        name: "FK_hinh_anh_cong_viec_id_cong_viec",
                        column: x => x.id_cong_viec,
                        principalTable: "cong_viec",
                        principalColumn: "id_cong_viec",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ung_tuyen",
                columns: table => new
                {
                    id_ung_tuyen = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_tim_viec = table.Column<long>(type: "bigint", nullable: false),
                    id_cong_viec = table.Column<long>(type: "bigint", nullable: false),
                    id_cv = table.Column<long>(type: "bigint", nullable: false),
                    ngay_ung_tuyen = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ung_tuyen", x => x.id_ung_tuyen);
                    table.ForeignKey(
                        name: "FK_ung_tuyen_cong_viec_id_cong_viec",
                        column: x => x.id_cong_viec,
                        principalTable: "cong_viec",
                        principalColumn: "id_cong_viec",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ung_tuyen_ho_so_tuyen_dung_id_cv",
                        column: x => x.id_cv,
                        principalTable: "ho_so_tuyen_dung",
                        principalColumn: "id_cv",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ung_tuyen_nguoi_tim_viec_id_tim_viec",
                        column: x => x.id_tim_viec,
                        principalTable: "nguoi_tim_viec",
                        principalColumn: "id_tim_viec",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lich_su_thanh_toan",
                columns: table => new
                {
                    id_thanh_toan = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_tuyen_dung = table.Column<long>(type: "bigint", nullable: false),
                    id_goi = table.Column<long>(type: "bigint", nullable: false),
                    gia = table.Column<int>(type: "int", nullable: false),
                    id_don_hang = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_thanh_toan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngan_hang_thanh_toan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_giao_dich = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_thanh_toan", x => x.id_thanh_toan);
                    table.ForeignKey(
                        name: "FK_lich_su_thanh_toan_goi_bai_dang_id_goi",
                        column: x => x.id_goi,
                        principalTable: "goi_bai_dang",
                        principalColumn: "id_goi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lich_su_thanh_toan_nha_tuyen_dung_id_tuyen_dung",
                        column: x => x.id_tuyen_dung,
                        principalTable: "nha_tuyen_dung",
                        principalColumn: "id_tuyen_dung",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "qua_trinh_lam_viec",
                columns: table => new
                {
                    id_lam_viec = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_ung_tuyen = table.Column<long>(type: "bigint", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qua_trinh_lam_viec", x => x.id_lam_viec);
                    table.ForeignKey(
                        name: "FK_qua_trinh_lam_viec_ung_tuyen_id_ung_tuyen",
                        column: x => x.id_ung_tuyen,
                        principalTable: "ung_tuyen",
                        principalColumn: "id_ung_tuyen",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cong_viec_id_danh_muc",
                table: "cong_viec",
                column: "id_danh_muc");

            migrationBuilder.CreateIndex(
                name: "IX_cong_viec_id_tuyen_dung",
                table: "cong_viec",
                column: "id_tuyen_dung");

            migrationBuilder.CreateIndex(
                name: "IX_danh_gia_id_cong_viec",
                table: "danh_gia",
                column: "id_cong_viec");

            migrationBuilder.CreateIndex(
                name: "IX_danh_gia_id_tim_viec",
                table: "danh_gia",
                column: "id_tim_viec");

            migrationBuilder.CreateIndex(
                name: "IX_danh_gia_id_tuyen_dung",
                table: "danh_gia",
                column: "id_tuyen_dung");

            migrationBuilder.CreateIndex(
                name: "IX_goi_bai_dang_id_quan_tri",
                table: "goi_bai_dang",
                column: "id_quan_tri");

            migrationBuilder.CreateIndex(
                name: "IX_hinh_anh_id_cong_viec",
                table: "hinh_anh",
                column: "id_cong_viec");

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_tuyen_dung_id_tim_viec",
                table: "ho_so_tuyen_dung",
                column: "id_tim_viec");

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_thanh_toan_id_goi",
                table: "lich_su_thanh_toan",
                column: "id_goi");

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_thanh_toan_id_tuyen_dung",
                table: "lich_su_thanh_toan",
                column: "id_tuyen_dung");

            migrationBuilder.CreateIndex(
                name: "IX_qua_trinh_lam_viec_id_ung_tuyen",
                table: "qua_trinh_lam_viec",
                column: "id_ung_tuyen");

            migrationBuilder.CreateIndex(
                name: "IX_ung_tuyen_id_cong_viec",
                table: "ung_tuyen",
                column: "id_cong_viec");

            migrationBuilder.CreateIndex(
                name: "IX_ung_tuyen_id_cv",
                table: "ung_tuyen",
                column: "id_cv");

            migrationBuilder.CreateIndex(
                name: "IX_ung_tuyen_id_tim_viec",
                table: "ung_tuyen",
                column: "id_tim_viec");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danh_gia");

            migrationBuilder.DropTable(
                name: "hinh_anh");

            migrationBuilder.DropTable(
                name: "lich_su_thanh_toan");

            migrationBuilder.DropTable(
                name: "qua_trinh_lam_viec");

            migrationBuilder.DropTable(
                name: "goi_bai_dang");

            migrationBuilder.DropTable(
                name: "ung_tuyen");

            migrationBuilder.DropTable(
                name: "quan_tri_vien");

            migrationBuilder.DropTable(
                name: "cong_viec");

            migrationBuilder.DropTable(
                name: "ho_so_tuyen_dung");

            migrationBuilder.DropTable(
                name: "danh_muc");

            migrationBuilder.DropTable(
                name: "nha_tuyen_dung");

            migrationBuilder.DropTable(
                name: "nguoi_tim_viec");

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Company = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salary = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
