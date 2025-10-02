using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatsFromVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stats",
                table: "Vehicles");

            migrationBuilder.AddColumn<float>(
                name: "Acceleration",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Accuracy",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "AimTime",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "Damage",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HP",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HullArmorFront",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HullArmorRear",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HullArmorSide",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Penetration",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "ReloadTime",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Speed",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TraverseSpeed",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TurretArmorFront",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TurretArmorRear",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TurretArmorSide",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "TurretTraverseSpeed",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acceleration",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Accuracy",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AimTime",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Damage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HP",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HullArmorFront",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HullArmorRear",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HullArmorSide",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Penetration",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ReloadTime",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TraverseSpeed",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TurretArmorFront",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TurretArmorRear",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TurretArmorSide",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TurretTraverseSpeed",
                table: "Vehicles");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Stats",
                table: "Vehicles",
                type: "jsonb",
                nullable: false);
        }
    }
}
