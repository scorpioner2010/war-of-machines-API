using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RavApi.Data;

namespace RavApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261128120000_RenameVehicleCatalogAndResearchTree")]
    public partial class RenameVehicleCatalogAndResearchTree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateReferenceTables(migrationBuilder);
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF (SELECT COUNT(*) FROM ""Vehicles"" v JOIN ""_RavVehicleNames"" n ON n.""Code"" = v.""Code"") <> 21 THEN
        RAISE EXCEPTION 'Vehicle catalog differs from the reviewed 21-code staging catalog; refusing rename.';
    END IF;
    IF EXISTS (
        SELECT 1 FROM ""_RavVehicleNames"" n
        LEFT JOIN ""Vehicles"" v ON v.""Code"" = n.""Code""
        WHERE v.""Id"" IS NULL OR v.""Name"" <> n.""PreviousName""
    ) THEN
        RAISE EXCEPTION 'Vehicle names differ from the reviewed staging catalog; refusing rename.';
    END IF;
    IF (SELECT COUNT(*)
        FROM ""VehicleResearchRequirements"" r
        JOIN ""Vehicles"" p ON p.""Id"" = r.""PredecessorVehicleId""
        JOIN ""Vehicles"" s ON s.""Id"" = r.""SuccessorVehicleId""
        JOIN ""_RavVehicleNames"" pn ON pn.""Code"" = p.""Code""
        JOIN ""_RavVehicleNames"" sn ON sn.""Code"" = s.""Code"") <> 19 THEN
        RAISE EXCEPTION 'Research graph differs from the reviewed staging graph; refusing replacement.';
    END IF;
    IF EXISTS (
        SELECT 1
        FROM ""_RavResearchBefore"" e
        JOIN ""Vehicles"" p ON p.""Code"" = e.""PredecessorCode""
        JOIN ""Vehicles"" s ON s.""Code"" = e.""SuccessorCode""
        LEFT JOIN ""VehicleResearchRequirements"" r
          ON r.""PredecessorVehicleId"" = p.""Id"" AND r.""SuccessorVehicleId"" = s.""Id""
        WHERE r.""PredecessorVehicleId"" IS NULL OR r.""RequiredXpOnPredecessor"" <> e.""RequiredXp""
    ) THEN
        RAISE EXCEPTION 'Research edge pairs or XP costs differ from the reviewed staging graph; refusing replacement.';
    END IF;
END $$;

UPDATE ""Vehicles"" v
SET ""Name"" = n.""NewName""
FROM ""_RavVehicleNames"" n
WHERE v.""Code"" = n.""Code"";

DELETE FROM ""VehicleResearchRequirements"" r
USING ""Vehicles"" p, ""Vehicles"" s, ""_RavVehicleNames"" pn, ""_RavVehicleNames"" sn
WHERE r.""PredecessorVehicleId"" = p.""Id""
  AND r.""SuccessorVehicleId"" = s.""Id""
  AND pn.""Code"" = p.""Code""
  AND sn.""Code"" = s.""Code"";

INSERT INTO ""VehicleResearchRequirements"" (""PredecessorVehicleId"", ""SuccessorVehicleId"", ""RequiredXpOnPredecessor"")
SELECT p.""Id"", s.""Id"", e.""RequiredXp""
FROM ""_RavResearchAfter"" e
JOIN ""Vehicles"" p ON p.""Code"" = e.""PredecessorCode""
JOIN ""Vehicles"" s ON s.""Code"" = e.""SuccessorCode"";
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            CreateReferenceTables(migrationBuilder);
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF (SELECT COUNT(*) FROM ""Vehicles"" v JOIN ""_RavVehicleNames"" n ON n.""Code"" = v.""Code"") <> 21 THEN
        RAISE EXCEPTION 'Vehicle catalog differs from the reviewed 21-code staging catalog; refusing rollback.';
    END IF;
    IF EXISTS (
        SELECT 1 FROM ""_RavVehicleNames"" n
        LEFT JOIN ""Vehicles"" v ON v.""Code"" = n.""Code""
        WHERE v.""Id"" IS NULL OR v.""Name"" <> n.""NewName""
    ) THEN
        RAISE EXCEPTION 'Vehicle names differ from the renamed staging catalog; refusing rollback.';
    END IF;
    IF (SELECT COUNT(*)
        FROM ""VehicleResearchRequirements"" r
        JOIN ""Vehicles"" p ON p.""Id"" = r.""PredecessorVehicleId""
        JOIN ""Vehicles"" s ON s.""Id"" = r.""SuccessorVehicleId""
        JOIN ""_RavVehicleNames"" pn ON pn.""Code"" = p.""Code""
        JOIN ""_RavVehicleNames"" sn ON sn.""Code"" = s.""Code"") <> 19 THEN
        RAISE EXCEPTION 'Research graph differs from the renamed staging graph; refusing rollback.';
    END IF;
    IF EXISTS (
        SELECT 1
        FROM ""_RavResearchAfter"" e
        JOIN ""Vehicles"" p ON p.""Code"" = e.""PredecessorCode""
        JOIN ""Vehicles"" s ON s.""Code"" = e.""SuccessorCode""
        LEFT JOIN ""VehicleResearchRequirements"" r
          ON r.""PredecessorVehicleId"" = p.""Id"" AND r.""SuccessorVehicleId"" = s.""Id""
        WHERE r.""PredecessorVehicleId"" IS NULL OR r.""RequiredXpOnPredecessor"" <> e.""RequiredXp""
    ) THEN
        RAISE EXCEPTION 'Research edge pairs or XP costs differ from the renamed staging graph; refusing rollback.';
    END IF;
END $$;

UPDATE ""Vehicles"" v
SET ""Name"" = n.""PreviousName""
FROM ""_RavVehicleNames"" n
WHERE v.""Code"" = n.""Code"";

DELETE FROM ""VehicleResearchRequirements"" r
USING ""Vehicles"" p, ""Vehicles"" s, ""_RavVehicleNames"" pn, ""_RavVehicleNames"" sn
WHERE r.""PredecessorVehicleId"" = p.""Id""
  AND r.""SuccessorVehicleId"" = s.""Id""
  AND pn.""Code"" = p.""Code""
  AND sn.""Code"" = s.""Code"";

INSERT INTO ""VehicleResearchRequirements"" (""PredecessorVehicleId"", ""SuccessorVehicleId"", ""RequiredXpOnPredecessor"")
SELECT p.""Id"", s.""Id"", e.""RequiredXp""
FROM ""_RavResearchBefore"" e
JOIN ""Vehicles"" p ON p.""Code"" = e.""PredecessorCode""
JOIN ""Vehicles"" s ON s.""Code"" = e.""SuccessorCode"";
");
        }

        private static void CreateReferenceTables(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TEMP TABLE ""_RavVehicleNames"" (
    ""Code"" text PRIMARY KEY,
    ""PreviousName"" text NOT NULL,
    ""NewName"" text NOT NULL
) ON COMMIT DROP;

INSERT INTO ""_RavVehicleNames"" (""Code"", ""PreviousName"", ""NewName"") VALUES
('ia_l1_starter', 'IA Skirmisher', 'TZ-18'),
('ia_l2_scout', 'IA Strider', 'TZ-23 101F'),
('ia_l2_guardian', 'IA Bulwark', 'TZ-42'),
('ia_l2_colossus', 'IA Juggernaut', 'YUG Titan'),
('ia_l3_scout_wraith', 'IA Wraith', 'R30-Halberd'),
('ia_l3_guardian_warder', 'IA Warder', 'BR-174 Deadlock'),
('ia_l3_guardian_rampart', 'IA Rampart', 'BR-160'),
('ia_l3_colossus_mastodon', 'IA Mastodon', 'R42-Glaive 7B'),
('ia_l4_guardian_citadel', 'IA Citadel', 'Blackline-09 Mk. II'),
('ia_l4_colossus_dreadnought', 'IA Dreadnought', 'V-88/R Northstar'),
('nv_l1_starter', 'Nova Wisp', 'N-16 Whitecap'),
('nv_l2_scout', 'Nova Flicker', 'N-17'),
('nv_l2_guardian', 'Nova Aegis', 'ARGUS-7B'),
('nv_l2_colossus', 'Nova Titan', 'Cormorant 8C'),
('nv_l3_scout_rift', 'Nova Rift', 'VX-44/2'),
('nv_l3_scout_specter', 'Nova Specter', 'ZR-22 Knifepoint'),
('nv_l4_scout_horizon', 'Nova Horizon', '77-V Seeker'),
('nv_l3_guardian_prism', 'Nova Prism', 'P-8 Null Compass'),
('nv_l3_colossus_harrow', 'Nova Harrow', 'S-404 Firebreak'),
('nv_l3_colossus_oblivion', 'Nova Oblivion', 'A-93 Graveward'),
('nv_l4_colossus_eclipse', 'Nova Eclipse', 'M-03 Sundown');

CREATE TEMP TABLE ""_RavResearchBefore"" (
    ""PredecessorCode"" text NOT NULL,
    ""SuccessorCode"" text NOT NULL,
    ""RequiredXp"" integer NOT NULL,
    PRIMARY KEY (""PredecessorCode"", ""SuccessorCode"")
) ON COMMIT DROP;

INSERT INTO ""_RavResearchBefore"" VALUES
('ia_l1_starter', 'ia_l2_scout', 400),
('ia_l1_starter', 'ia_l2_guardian', 700),
('ia_l1_starter', 'ia_l2_colossus', 1000),
('ia_l2_scout', 'ia_l3_scout_wraith', 1800),
('ia_l2_guardian', 'ia_l3_guardian_warder', 2200),
('ia_l2_guardian', 'ia_l3_guardian_rampart', 2600),
('ia_l2_colossus', 'ia_l3_colossus_mastodon', 3200),
('ia_l3_guardian_warder', 'ia_l4_guardian_citadel', 5000),
('ia_l3_colossus_mastodon', 'ia_l4_colossus_dreadnought', 6200),
('nv_l1_starter', 'nv_l2_scout', 400),
('nv_l1_starter', 'nv_l2_guardian', 700),
('nv_l1_starter', 'nv_l2_colossus', 1000),
('nv_l2_scout', 'nv_l3_scout_rift', 1700),
('nv_l2_scout', 'nv_l3_scout_specter', 2200),
('nv_l3_scout_rift', 'nv_l4_scout_horizon', 4800),
('nv_l2_guardian', 'nv_l3_guardian_prism', 2400),
('nv_l2_colossus', 'nv_l3_colossus_harrow', 3000),
('nv_l2_colossus', 'nv_l3_colossus_oblivion', 3600),
('nv_l3_colossus_harrow', 'nv_l4_colossus_eclipse', 6500);

CREATE TEMP TABLE ""_RavResearchAfter"" (
    ""PredecessorCode"" text NOT NULL,
    ""SuccessorCode"" text NOT NULL,
    ""RequiredXp"" integer NOT NULL,
    PRIMARY KEY (""PredecessorCode"", ""SuccessorCode"")
) ON COMMIT DROP;

INSERT INTO ""_RavResearchAfter"" VALUES
('ia_l1_starter', 'ia_l2_scout', 400),
('ia_l2_scout', 'ia_l2_guardian', 700),
('ia_l2_guardian', 'ia_l2_colossus', 1000),
('ia_l1_starter', 'ia_l3_guardian_rampart', 2600),
('ia_l3_guardian_rampart', 'ia_l3_guardian_warder', 2200),
('ia_l3_guardian_warder', 'ia_l3_scout_wraith', 1800),
('ia_l3_guardian_rampart', 'ia_l3_colossus_mastodon', 3200),
('ia_l3_colossus_mastodon', 'ia_l4_colossus_dreadnought', 6200),
('ia_l4_colossus_dreadnought', 'ia_l4_guardian_citadel', 5000),
('nv_l1_starter', 'nv_l2_scout', 400),
('nv_l2_scout', 'nv_l2_guardian', 700),
('nv_l2_guardian', 'nv_l2_colossus', 1000),
('nv_l2_colossus', 'nv_l3_scout_specter', 2200),
('nv_l1_starter', 'nv_l3_guardian_prism', 2400),
('nv_l3_guardian_prism', 'nv_l3_scout_rift', 1700),
('nv_l3_scout_rift', 'nv_l4_colossus_eclipse', 6500),
('nv_l3_scout_rift', 'nv_l4_scout_horizon', 4800),
('nv_l1_starter', 'nv_l3_colossus_oblivion', 3600),
('nv_l3_colossus_oblivion', 'nv_l3_colossus_harrow', 3000);
");
        }
    }
}
