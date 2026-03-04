using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccessLayer_FranLink.Migrations
{
    /// <inheritdoc />
    public partial class managmentInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_FranchiseStores_FranchiseStoreId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "IsCentralKitchen",
                table: "FranchiseStores");

            migrationBuilder.AlterColumn<int>(
                name: "FranchiseStoreId",
                table: "Inventories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "Inventories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CentralKitchenId",
                table: "Inventories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxThreshold",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinThreshold",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CentralKitchens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Capacity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentralKitchens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryDisposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisposedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDisposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryDisposals_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryDisposals_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryDisposals_Users_DisposedByUserId",
                        column: x => x.DisposedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromCentralKitchenId = table.Column<int>(type: "integer", nullable: true),
                    FromStoreId = table.Column<int>(type: "integer", nullable: true),
                    ToCentralKitchenId = table.Column<int>(type: "integer", nullable: true),
                    ToStoreId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_CentralKitchens_FromCentralKitchenId",
                        column: x => x.FromCentralKitchenId,
                        principalTable: "CentralKitchens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_CentralKitchens_ToCentralKitchenId",
                        column: x => x.ToCentralKitchenId,
                        principalTable: "CentralKitchens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_FranchiseStores_FromStoreId",
                        column: x => x.FromStoreId,
                        principalTable: "FranchiseStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_FranchiseStores_ToStoreId",
                        column: x => x.ToStoreId,
                        principalTable: "FranchiseStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransfers_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CentralKitchenId",
                table: "Inventories",
                column: "CentralKitchenId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDisposals_ApprovedByUserId",
                table: "InventoryDisposals",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDisposals_DisposedByUserId",
                table: "InventoryDisposals",
                column: "DisposedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDisposals_InventoryId",
                table: "InventoryDisposals",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_ApprovedByUserId",
                table: "InventoryTransfers",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_FromCentralKitchenId",
                table: "InventoryTransfers",
                column: "FromCentralKitchenId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_FromStoreId",
                table: "InventoryTransfers",
                column: "FromStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_ProductId",
                table: "InventoryTransfers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_RequestedByUserId",
                table: "InventoryTransfers",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_ToCentralKitchenId",
                table: "InventoryTransfers",
                column: "ToCentralKitchenId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransfers_ToStoreId",
                table: "InventoryTransfers",
                column: "ToStoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_CentralKitchens_CentralKitchenId",
                table: "Inventories",
                column: "CentralKitchenId",
                principalTable: "CentralKitchens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_FranchiseStores_FranchiseStoreId",
                table: "Inventories",
                column: "FranchiseStoreId",
                principalTable: "FranchiseStores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_CentralKitchens_CentralKitchenId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_FranchiseStores_FranchiseStoreId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "InventoryDisposals");

            migrationBuilder.DropTable(
                name: "InventoryTransfers");

            migrationBuilder.DropTable(
                name: "CentralKitchens");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_CentralKitchenId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CentralKitchenId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MaxThreshold",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MinThreshold",
                table: "Inventories");

            migrationBuilder.AlterColumn<int>(
                name: "FranchiseStoreId",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCentralKitchen",
                table: "FranchiseStores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_FranchiseStores_FranchiseStoreId",
                table: "Inventories",
                column: "FranchiseStoreId",
                principalTable: "FranchiseStores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
