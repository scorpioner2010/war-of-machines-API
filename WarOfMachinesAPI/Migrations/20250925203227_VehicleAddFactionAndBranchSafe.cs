using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    public partial class VehicleAddFactionAndBranchSafe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Додаємо нові колонки без жорстких обмежень спочатку
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Vehicles",
                type: "text",
                nullable: false,
                defaultValue: "tracked");

            migrationBuilder.AddColumn<int>(
                name: "FactionId",
                table: "Vehicles",
                type: "integer",
                nullable: true); // <-- тимчасово nullable

            // 2) Переконаємось, що фракції існують (INSERT ... ON CONFLICT DO NOTHING)
            migrationBuilder.Sql(@"
INSERT INTO ""Factions"" (""Code"", ""Name"", ""Description"")
VALUES 
 ('iron_alliance','Залізний Альянс','Фракція важких мехів і сталі'),
 ('nova_syndicate','Нова Синдикат','Фракція високих технологій і мобільності')
ON CONFLICT (""Code"") DO NOTHING;
");

            // 3) Проставимо існуючим Vehicle валідну фракцію (за замовчуванням iron_alliance)
            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""FactionId"" = f.""Id""
FROM ""Factions"" f
WHERE ""Vehicles"".""FactionId"" IS NULL
  AND f.""Code"" = 'iron_alliance';
");

            // 4) Робимо NOT NULL після заповнення
            migrationBuilder.Sql(@"ALTER TABLE ""Vehicles"" ALTER COLUMN ""FactionId"" SET NOT NULL;");

            // 5) Індекс + зовнішній ключ
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FactionId",
                table: "Vehicles",
                column: "FactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // не каскадимо видалення фракції
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Factions_FactionId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_FactionId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FactionId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Vehicles");
        }
    }
}
