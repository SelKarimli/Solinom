using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.MVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRtdTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RTDs_Tables_TableId",
                table: "RTDs");

            migrationBuilder.DropIndex(
                name: "IX_RTDs_TableId",
                table: "RTDs");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "RTDs");

            migrationBuilder.AddColumn<bool>(
                name: "Reserved",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reserved",
                table: "Tables");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "RTDs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RTDs_TableId",
                table: "RTDs",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_RTDs_Tables_TableId",
                table: "RTDs",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id");
        }
    }
}
