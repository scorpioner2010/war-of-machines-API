using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RavApi.Data;

#nullable disable

namespace RavApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261103120000_AddVehicleEnginePower")]
    public partial class AddVehicleEnginePower : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "EnginePower",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 1f);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_EnginePower_Positive",
                table: "Vehicles",
                sql: "\"EnginePower\" > 0");

            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""EnginePower"" = CASE ""Code""
    WHEN 'ia_l1_starter' THEN 700
    WHEN 'ia_l2_scout' THEN 860
    WHEN 'ia_l2_guardian' THEN 820
    WHEN 'ia_l2_colossus' THEN 940
    WHEN 'ia_l3_scout_wraith' THEN 930
    WHEN 'ia_l3_guardian_warder' THEN 1060
    WHEN 'ia_l3_guardian_rampart' THEN 1020
    WHEN 'ia_l3_colossus_mastodon' THEN 1180
    WHEN 'ia_l4_guardian_citadel' THEN 1160
    WHEN 'ia_l4_colossus_dreadnought' THEN 1320
    WHEN 'nv_l1_starter' THEN 360
    WHEN 'nv_l2_scout' THEN 420
    WHEN 'nv_l2_guardian' THEN 460
    WHEN 'nv_l2_colossus' THEN 520
    WHEN 'nv_l3_scout_rift' THEN 500
    WHEN 'nv_l3_scout_specter' THEN 510
    WHEN 'nv_l4_scout_horizon' THEN 560
    WHEN 'nv_l3_guardian_prism' THEN 540
    WHEN 'nv_l3_colossus_harrow' THEN 600
    WHEN 'nv_l3_colossus_oblivion' THEN 630
    WHEN 'nv_l4_colossus_eclipse' THEN 700
    ELSE ""EnginePower""
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_EnginePower_Positive",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "EnginePower",
                table: "Vehicles");
        }
    }
}
