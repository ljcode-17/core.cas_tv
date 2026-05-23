using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAS.FinancePortal.Web.Server.Persistence.Migrations.CurrentApplication
{
    /// <inheritdoc />
    public partial class ConvertEmployeeDepartmentToViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_Vw_Employees",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_vw_combined_user_informations_Vw_Departments",
                table: "vw_combined_user_informations");

            migrationBuilder.DropTable(
                name: "Vw_Employees");

            migrationBuilder.DropTable(
                name: "Vw_Departments");

            migrationBuilder.Sql(
                """
                CREATE VIEW [dbo].[Vw_Departments]
                AS
                SELECT
                    CAST(NULL AS int) AS [Id],
                    CAST(NULL AS int) AS [DepartmentGroupId],
                    CAST(NULL AS nvarchar(1000)) AS [DepartmentName],
                    CAST(NULL AS nvarchar(500)) AS [Alias],
                    CAST(NULL AS nvarchar(50)) AS [CreatedBy],
                    CAST(NULL AS datetime) AS [CreatedDate],
                    CAST(NULL AS int) AS [WeekStart],
                    CAST(NULL AS nvarchar(255)) AS [Timezone],
                    CAST(NULL AS nvarchar(6)) AS [TimeDifferenceMnl]
                WHERE 1 = 0;
                """);

            migrationBuilder.Sql(
                """
                CREATE VIEW [dbo].[Vw_Employees]
                AS
                SELECT
                    CAST(NULL AS int) AS [EmployeeId],
                    CAST(NULL AS nvarchar(500)) AS [FirstName],
                    CAST(NULL AS nvarchar(500)) AS [LastName],
                    CAST(NULL AS int) AS [DepartmentId],
                    CAST(NULL AS nvarchar(50)) AS [CompanyEmail],
                    CAST(NULL AS nvarchar(50)) AS [MobileNo],
                    CAST(NULL AS nvarchar(50)) AS [TIN],
                    CAST(NULL AS nvarchar(50)) AS [Gender],
                    CAST(NULL AS date) AS [DateHired],
                    CAST(NULL AS nvarchar(50)) AS [YearId],
                    CAST(NULL AS bit) AS [IsActive]
                WHERE 1 = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS [dbo].[Vw_Employees];");
            migrationBuilder.Sql("DROP VIEW IF EXISTS [dbo].[Vw_Departments];");

            migrationBuilder.CreateTable(
                name: "Vw_Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DepartmentGroupId = table.Column<int>(type: "int", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TimeDifferenceMnl = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WeekStart = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vw_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vw_Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CompanyEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateHired = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YearId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Vw_Employees_DepartmentId",
                table: "Vw_Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_Vw_Employees",
                table: "Signatures",
                column: "EmployeeId",
                principalTable: "Vw_Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_vw_combined_user_informations_Vw_Departments",
                table: "vw_combined_user_informations",
                column: "DepartmentId",
                principalTable: "Vw_Departments",
                principalColumn: "Id");
        }
    }
}
