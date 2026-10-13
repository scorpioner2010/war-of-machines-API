using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WarOfMachines.Data;

#nullable disable

namespace WarOfMachines.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261014120000_AddVehicleTraverseAcceleration")]
    public partial class AddVehicleTraverseAcceleration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TraverseAcceleration",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""TraverseAcceleration"" = ""Acceleration"";
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraverseAcceleration",
                table: "Vehicles");
        }
    }
}
