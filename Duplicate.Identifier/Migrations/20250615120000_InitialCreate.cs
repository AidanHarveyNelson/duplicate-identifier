using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Duplicate.Identifier.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemIngestion",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngestionTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemIngestion", x => new { x.ItemId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemIngestion_ItemId",
                table: "ItemIngestion",
                column: "ItemId");

            migrationBuilder.CreateTable(
                name: "ScanResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishTime = table.Column<DateTime>(nullable: false),
                    LastModificationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanResults_Id",
                table: "ScanResults",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemIngestion");

            migrationBuilder.DropTable(
                name: "ScanResults");
        }
    }
}
