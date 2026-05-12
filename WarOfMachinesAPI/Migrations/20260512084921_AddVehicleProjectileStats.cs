using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleProjectileStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Damage",
                table: "Vehicles");

            migrationBuilder.AddColumn<float>(
                name: "DamageMax",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 110f);

            migrationBuilder.AddColumn<float>(
                name: "DamageMin",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 90f);

            migrationBuilder.AddColumn<float>(
                name: "ShellSpeed",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 70f);

            migrationBuilder.AddColumn<int>(
                name: "ShellsCount",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_DamageMin_LessOrEqual_DamageMax",
                table: "Vehicles",
                sql: "\"DamageMin\" <= \"DamageMax\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_ShellsCount_NonNegative",
                table: "Vehicles",
                sql: "\"ShellsCount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_ShellSpeed_Positive",
                table: "Vehicles",
                sql: "\"ShellSpeed\" > 0");

            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET
    ""ShellSpeed"" = CASE
        WHEN ""Class"" = 0 AND ""Level"" <= 1 THEN 84
        WHEN ""Class"" = 0 THEN 90
        WHEN ""Class"" = 1 THEN 75
        WHEN ""Class"" = 2 THEN 55
        ELSE 70
    END,
    ""ShellsCount"" = CASE
        WHEN ""Class"" = 0 AND ""Level"" <= 1 THEN 32
        WHEN ""Class"" = 0 THEN 40
        WHEN ""Class"" = 1 THEN 30
        WHEN ""Class"" = 2 THEN 18
        ELSE 20
    END,
    ""DamageMin"" = CASE
        WHEN ""Class"" = 0 AND ""Level"" <= 1 THEN 40
        WHEN ""Class"" = 0 THEN 45
        WHEN ""Class"" = 1 THEN 80
        WHEN ""Class"" = 2 THEN 140
        ELSE 90
    END,
    ""DamageMax"" = CASE
        WHEN ""Class"" = 0 AND ""Level"" <= 1 THEN 58
        WHEN ""Class"" = 0 THEN 65
        WHEN ""Class"" = 1 THEN 120
        WHEN ""Class"" = 2 THEN 210
        ELSE 110
    END;

UPDATE ""Vehicles""
SET ""ShellSpeed"" = 92, ""ShellsCount"" = 30, ""DamageMin"" = 42, ""DamageMax"" = 60
WHERE ""Code"" = 'nv_l1_starter';

UPDATE ""Vehicles""
SET ""ShellSpeed"" = 100, ""ShellsCount"" = 36, ""DamageMin"" = 50, ""DamageMax"" = 70
WHERE ""Code"" = 'nv_l2_scout';

UPDATE ""Vehicles""
SET ""ShellSpeed"" = 82, ""ShellsCount"" = 26, ""DamageMin"" = 88, ""DamageMax"" = 125
WHERE ""Code"" = 'nv_l2_guardian';

UPDATE ""Vehicles""
SET ""ShellSpeed"" = 62, ""ShellsCount"" = 16, ""DamageMin"" = 150, ""DamageMax"" = 220
WHERE ""Code"" = 'nv_l2_colossus';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_DamageMin_LessOrEqual_DamageMax",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_ShellsCount_NonNegative",
                table: "Vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_ShellSpeed_Positive",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DamageMax",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DamageMin",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ShellSpeed",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ShellsCount",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "Damage",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
