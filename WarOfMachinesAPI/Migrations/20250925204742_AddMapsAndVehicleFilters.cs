using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class AddMapsAndVehicleFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles");

            migrationBuilder.CreateTable(
                name: "Maps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Maps_Code",
                table: "Maps",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "Maps");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
