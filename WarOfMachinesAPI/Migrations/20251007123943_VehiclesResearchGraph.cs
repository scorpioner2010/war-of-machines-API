using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class VehiclesResearchGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Class",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrefabCode",
                table: "Vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseCost",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VehicleResearchRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredecessorVehicleId = table.Column<int>(type: "integer", nullable: false),
                    SuccessorVehicleId = table.Column<int>(type: "integer", nullable: false),
                    RequiredXpOnPredecessor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleResearchRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleResearchRequirements_Vehicles_PredecessorVehicleId",
                        column: x => x.PredecessorVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleResearchRequirements_Vehicles_SuccessorVehicleId",
                        column: x => x.SuccessorVehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleResearchRequirements_PredecessorVehicleId_SuccessorV~",
                table: "VehicleResearchRequirements",
                columns: new[] { "PredecessorVehicleId", "SuccessorVehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleResearchRequirements_SuccessorVehicleId",
                table: "VehicleResearchRequirements",
                column: "SuccessorVehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleResearchRequirements");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PrefabCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PurchaseCost",
                table: "Vehicles");
        }
    }
}
