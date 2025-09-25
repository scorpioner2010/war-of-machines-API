using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrenciesToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Adamant",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bolts",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adamant",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Bolts",
                table: "Players");
        }
    }
}
