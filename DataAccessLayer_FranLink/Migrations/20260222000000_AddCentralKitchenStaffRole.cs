using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer_FranLink.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralKitchenStaffRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""RoleName"")
                SELECT 'CentralKitchenStaff'
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""RoleName"" = 'CentralKitchenStaff');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Roles"" WHERE ""RoleName"" = 'CentralKitchenStaff';");
        }
    }
}
