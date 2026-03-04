using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer_FranLink.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralKitchenToInternalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CentralKitchenId",
                table: "InternalOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternalOrders_CentralKitchenId",
                table: "InternalOrders",
                column: "CentralKitchenId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalOrders_CentralKitchens_CentralKitchenId",
                table: "InternalOrders",
                column: "CentralKitchenId",
                principalTable: "CentralKitchens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalOrders_CentralKitchens_CentralKitchenId",
                table: "InternalOrders");

            migrationBuilder.DropIndex(
                name: "IX_InternalOrders_CentralKitchenId",
                table: "InternalOrders");

            migrationBuilder.DropColumn(
                name: "CentralKitchenId",
                table: "InternalOrders");
        }
    }
}
