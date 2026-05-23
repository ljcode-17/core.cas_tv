using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAS.FinancePortal.Web.Server.Persistence.Migrations.CurrentApplication
{
    /// <inheritdoc />
    public partial class InitialLocalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_access_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    access_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_access", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Vw_Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentGroupId = table.Column<int>(type: "int", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    WeekStart = table.Column<int>(type: "int", nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TimeDifferenceMnl = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vw_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_menus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_access_type_id = table.Column<int>(type: "int", nullable: false),
                    menu_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    menu_path = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    menu_icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    is_parent = table.Column<bool>(type: "bit", nullable: true),
                    is_sub_parent = table.Column<bool>(type: "bit", nullable: true),
                    order_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_menus", x => x.id);
                    table.ForeignKey(
                        name: "FK_system_menus_user_access_types",
                        column: x => x.user_access_type_id,
                        principalTable: "user_access_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_access",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    user_access_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_access_1", x => x.id);
                    table.UniqueConstraint("AK_user_access_user_id", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_access_user_access_types",
                        column: x => x.user_access_id,
                        principalTable: "user_access_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Vw_Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CompanyEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateHired = table.Column<DateOnly>(type: "date", nullable: false),
                    YearId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vw_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Vw_Employees_Vw_Departments",
                        column: x => x.DepartmentId,
                        principalTable: "Vw_Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "Active"),
                    UploadedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_user_access_user_id",
                        column: x => x.UploadedBy,
                        principalTable: "user_access",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "vw_combined_user_informations",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    user_email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    user_type = table.Column<int>(type: "int", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_lock_out = table.Column<bool>(type: "bit", nullable: false),
                    login_attemps = table.Column<int>(type: "int", nullable: false),
                    user_reference_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    last_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vw_combined_user_informations", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_vw_combined_user_informations_Vw_Departments",
                        column: x => x.DepartmentId,
                        principalTable: "Vw_Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_vw_combined_user_informations_user_access_user_id",
                        column: x => x.user_id,
                        principalTable: "user_access",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ESignature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_Vw_Employees",
                        column: x => x.EmployeeId,
                        principalTable: "Vw_Employees",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_EmployeeId",
                table: "Signatures",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_system_menus_user_access_type_id",
                table: "system_menus",
                column: "user_access_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_access_user_access_id",
                table: "user_access",
                column: "user_access_id");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CreatedAt",
                table: "Videos",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_Status",
                table: "Videos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_UploadedBy",
                table: "Videos",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_vw_combined_user_informations_DepartmentId",
                table: "vw_combined_user_informations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vw_Employees_DepartmentId",
                table: "Vw_Employees",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropTable(
                name: "system_menus");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "vw_combined_user_informations");

            migrationBuilder.DropTable(
                name: "Vw_Employees");

            migrationBuilder.DropTable(
                name: "user_access");

            migrationBuilder.DropTable(
                name: "Vw_Departments");

            migrationBuilder.DropTable(
                name: "user_access_types");
        }
    }
}
