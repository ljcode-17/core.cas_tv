using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAS.FinancePortal.Web.Server.Persistence.Migrations.CurrentApplication
{
    public partial class ConvertVwCombinedToView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints referencing the table if exist
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_vw_combined_user_informations_Vw_Departments')
    ALTER TABLE [vw_combined_user_informations] DROP CONSTRAINT FK_vw_combined_user_informations_Vw_Departments;");

            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_vw_combined_user_informations_user_access_user_id')
    ALTER TABLE [vw_combined_user_informations] DROP CONSTRAINT FK_vw_combined_user_informations_user_access_user_id;");

            // Drop the table and recreate as a view shell
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.vw_combined_user_informations', 'U') IS NOT NULL
    DROP TABLE dbo.vw_combined_user_informations;

CREATE VIEW dbo.vw_combined_user_informations AS
SELECT
    CAST(NULL AS int) AS user_id,
    CAST('' AS nvarchar(max)) AS user_email,
    CAST(0 AS int) AS user_type,
    CAST('' AS nvarchar(max)) AS password,
    CAST(0 AS bit) AS is_active,
    CAST(0 AS bit) AS is_lock_out,
    CAST(0 AS int) AS login_attemps,
    CAST(0 AS int) AS user_reference_id,
    CAST(NULL AS nvarchar(500)) AS first_name,
    CAST(NULL AS nvarchar(500)) AS last_name,
    CAST(NULL AS int) AS DepartmentId
WHERE 1 = 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.vw_combined_user_informations', 'V') IS NOT NULL
    DROP VIEW dbo.vw_combined_user_informations;

CREATE TABLE dbo.vw_combined_user_informations (
    user_id int NOT NULL PRIMARY KEY,
    user_email nvarchar(max) NOT NULL,
    user_type int NOT NULL,
    password nvarchar(max) NOT NULL,
    is_active bit NOT NULL,
    is_lock_out bit NOT NULL,
    login_attemps int NOT NULL,
    user_reference_id int NOT NULL,
    first_name nvarchar(500) NULL,
    last_name nvarchar(500) NULL,
    DepartmentId int NOT NULL
);");
        }
    }
}
