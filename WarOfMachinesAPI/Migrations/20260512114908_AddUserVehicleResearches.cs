using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class AddUserVehicleResearches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserVehicleResearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ResearchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVehicleResearches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVehicleResearches_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVehicleResearches_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicleResearches_UserId_VehicleId",
                table: "UserVehicleResearches",
                columns: new[] { "UserId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicleResearches_VehicleId",
                table: "UserVehicleResearches",
                column: "VehicleId");

            migrationBuilder.Sql(@"
INSERT INTO ""UserVehicleResearches"" (""UserId"", ""VehicleId"", ""ResearchedAt"")
SELECT DISTINCT ""UserId"", ""VehicleId"", CURRENT_TIMESTAMP
FROM ""UserVehicles""
ON CONFLICT (""UserId"", ""VehicleId"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVehicleResearches");
        }
    }
}
