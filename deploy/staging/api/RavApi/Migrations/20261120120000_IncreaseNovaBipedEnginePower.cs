using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RavApi.Data;

namespace RavApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120120000_IncreaseNovaBipedEnginePower")]
    public partial class IncreaseNovaBipedEnginePower : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""EnginePower"" = CASE ""Code""
    WHEN 'nv_l1_starter' THEN 1360
    WHEN 'nv_l2_scout' THEN 1420
    WHEN 'nv_l2_guardian' THEN 1460
    WHEN 'nv_l2_colossus' THEN 1520
    WHEN 'nv_l3_scout_rift' THEN 1500
    WHEN 'nv_l3_scout_specter' THEN 1510
    WHEN 'nv_l4_scout_horizon' THEN 1560
    WHEN 'nv_l3_guardian_prism' THEN 1540
    WHEN 'nv_l3_colossus_harrow' THEN 1600
    WHEN 'nv_l3_colossus_oblivion' THEN 1630
    WHEN 'nv_l4_colossus_eclipse' THEN 1700
    ELSE ""EnginePower""
END
WHERE ""Branch"" = 'biped'
  AND ""Code"" IN (
      'nv_l1_starter',
      'nv_l2_scout',
      'nv_l2_guardian',
      'nv_l2_colossus',
      'nv_l3_scout_rift',
      'nv_l3_scout_specter',
      'nv_l4_scout_horizon',
      'nv_l3_guardian_prism',
      'nv_l3_colossus_harrow',
      'nv_l3_colossus_oblivion',
      'nv_l4_colossus_eclipse'
  );
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""EnginePower"" = CASE ""Code""
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
END
WHERE ""Branch"" = 'biped'
  AND ""Code"" IN (
      'nv_l1_starter',
      'nv_l2_scout',
      'nv_l2_guardian',
      'nv_l2_colossus',
      'nv_l3_scout_rift',
      'nv_l3_scout_specter',
      'nv_l4_scout_horizon',
      'nv_l3_guardian_prism',
      'nv_l3_colossus_harrow',
      'nv_l3_colossus_oblivion',
      'nv_l4_colossus_eclipse'
  );
");
        }
    }
}
