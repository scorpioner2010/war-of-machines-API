using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RavApi.Data;

namespace RavApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261128150000_RebalanceHiddenRobotStats")]
    public partial class RebalanceHiddenRobotStats : Migration
    {
        private const string BalanceRowsJson = """
        [
          {"Code":"ia_l3_scout_wraith","Level":3,"Class":0,"Original":{"ShellsCount":170,"DamageMin":34.8,"DamageMax":52.2,"Penetration":102,"ViewRange":135,"AimTime":0.95,"TurretArmorFront":58,"TurretArmorSide":36,"TurretArmorRear":25,"HullArmorFront":53,"HullArmorSide":33,"HullArmorRear":23},"Balanced":{"ShellsCount":180,"DamageMin":38.016,"DamageMax":57.024,"Penetration":108,"ViewRange":140,"AimTime":0.9,"TurretArmorFront":62,"TurretArmorSide":38,"TurretArmorRear":26,"HullArmorFront":56,"HullArmorSide":35,"HullArmorRear":24}},
          {"Code":"ia_l3_guardian_warder","Level":3,"Class":1,"Original":{"HP":320,"ShellsCount":90,"Penetration":125,"ViewRange":108,"Accuracy":1.45,"AimTime":2.15,"TurretArmorFront":88,"TurretArmorSide":56,"TurretArmorRear":38,"HullArmorFront":80,"HullArmorSide":52,"HullArmorRear":36},"Balanced":{"HP":330,"ShellsCount":100,"Penetration":132,"ViewRange":112,"Accuracy":1.35,"AimTime":2,"TurretArmorFront":92,"TurretArmorSide":59,"TurretArmorRear":40,"HullArmorFront":84,"HullArmorSide":55,"HullArmorRear":38}},
          {"Code":"ia_l3_guardian_rampart","Level":3,"Class":1,"Original":{"ShellsCount":80,"Penetration":130,"ViewRange":102,"Accuracy":1.55,"AimTime":2.3,"TurretArmorFront":100,"TurretArmorSide":64,"TurretArmorRear":43,"HullArmorFront":91,"HullArmorSide":59,"HullArmorRear":40},"Balanced":{"ShellsCount":90,"Penetration":136,"ViewRange":108,"Accuracy":1.45,"AimTime":2.2,"TurretArmorFront":103,"TurretArmorSide":66,"TurretArmorRear":45,"HullArmorFront":94,"HullArmorSide":61,"HullArmorRear":41}},
          {"Code":"ia_l3_colossus_mastodon","Level":3,"Class":2,"Original":{"HP":430,"ShellSpeed":96,"ShellsCount":58,"DamageMin":145.2,"DamageMax":217.8,"ViewRange":88,"Accuracy":1.9,"AimTime":3.6,"TurretArmorFront":130,"TurretArmorSide":82,"TurretArmorRear":56,"HullArmorFront":116,"HullArmorSide":75,"HullArmorRear":52},"Balanced":{"HP":440,"ShellSpeed":100,"ShellsCount":60,"DamageMin":162,"DamageMax":243,"Penetration":148,"ViewRange":92,"Accuracy":1.85,"AimTime":3.3,"TurretArmorFront":136,"TurretArmorSide":86,"TurretArmorRear":59,"HullArmorFront":123,"HullArmorSide":80,"HullArmorRear":55}},
          {"Code":"ia_l4_guardian_citadel","Level":4,"Class":1,"Original":{"HP":445,"ShellSpeed":126,"ShellsCount":95,"DamageMin":120,"DamageMax":180,"Penetration":140,"ViewRange":110,"ReloadTime":2.75,"Accuracy":1.6,"AimTime":2.05,"TurretArmorFront":118,"TurretArmorSide":75,"TurretArmorRear":50,"HullArmorFront":108,"HullArmorSide":70,"HullArmorRear":48},"Balanced":{"HP":455,"ShellSpeed":132,"ShellsCount":105,"DamageMin":126,"DamageMax":189,"Penetration":150,"ViewRange":120,"ReloadTime":2.8,"Accuracy":1.28,"AimTime":1.8,"TurretArmorFront":124,"TurretArmorSide":79,"TurretArmorRear":54,"HullArmorFront":113,"HullArmorSide":73,"HullArmorRear":50}},
          {"Code":"ia_l4_colossus_dreadnought","Level":4,"Class":2,"Original":{"HP":560,"ShellSpeed":92,"ShellsCount":52,"DamageMin":172.8,"DamageMax":259.2,"Penetration":155,"ViewRange":90,"Accuracy":2,"AimTime":3.9,"TurretArmorFront":160,"TurretArmorSide":100,"TurretArmorRear":68,"HullArmorFront":144,"HullArmorSide":93,"HullArmorRear":64},"Balanced":{"HP":565,"ShellSpeed":96,"ShellsCount":55,"DamageMin":207.36,"DamageMax":311.04,"Penetration":165,"ViewRange":96,"Accuracy":1.9,"AimTime":3.6,"Speed":2.65,"Acceleration":0.57,"TraverseAcceleration":57,"TraverseSpeed":26.5,"TurretTraverseSpeed":11,"TurretArmorFront":168,"TurretArmorSide":107,"TurretArmorRear":73,"HullArmorFront":151,"HullArmorSide":98,"HullArmorRear":67}},
          {"Code":"nv_l3_scout_rift","Level":3,"Class":0,"Original":{"HP":220,"ShellsCount":205,"DamageMin":33.6,"DamageMax":50.4,"Penetration":104,"ViewRange":140,"ReloadTime":0.95,"Accuracy":0.8,"AimTime":0.85,"TurretArmorFront":54,"TurretArmorSide":34,"TurretArmorRear":23,"HullArmorFront":49,"HullArmorSide":31,"HullArmorRear":21},"Balanced":{"HP":225,"ShellsCount":215,"DamageMin":36,"DamageMax":54,"Penetration":108,"ViewRange":142,"ReloadTime":1,"Accuracy":0.78,"AimTime":0.82,"TurretArmorFront":58,"TurretArmorSide":37,"TurretArmorRear":25,"HullArmorFront":53,"HullArmorSide":34,"HullArmorRear":23}},
          {"Code":"nv_l3_scout_specter","Level":3,"Class":0,"Original":{"HP":205,"ShellsCount":220,"DamageMin":30,"DamageMax":45,"Penetration":101,"Accuracy":0.8,"AimTime":0.8,"TurretArmorFront":50,"TurretArmorSide":32,"TurretArmorRear":22,"HullArmorFront":46,"HullArmorSide":29,"HullArmorRear":20},"Balanced":{"HP":220,"ShellsCount":230,"DamageMin":30.6,"DamageMax":45.9,"Penetration":106,"Accuracy":0.77,"AimTime":0.78,"TurretArmorFront":55,"TurretArmorSide":35,"TurretArmorRear":24,"HullArmorFront":50,"HullArmorSide":32,"HullArmorRear":22}},
          {"Code":"nv_l4_scout_horizon","Level":4,"Class":0,"Original":{"HP":275,"ShellsCount":235,"DamageMin":36,"DamageMax":54,"Penetration":110,"ViewRange":155,"ReloadTime":0.85,"Accuracy":0.8,"AimTime":0.75,"TurretArmorFront":65,"TurretArmorSide":41,"TurretArmorRear":28,"HullArmorFront":59,"HullArmorSide":38,"HullArmorRear":26},"Balanced":{"HP":280,"ShellsCount":245,"DamageMin":43.2,"DamageMax":64.8,"Penetration":116,"ViewRange":158,"ReloadTime":1,"Accuracy":0.74,"AimTime":0.7,"TurretArmorFront":70,"TurretArmorSide":44,"TurretArmorRear":30,"HullArmorFront":64,"HullArmorSide":41,"HullArmorRear":28}},
          {"Code":"nv_l3_guardian_prism","Level":3,"Class":1,"Original":{"HP":315,"ShellsCount":95,"DamageMin":90,"DamageMax":135,"Penetration":130,"ReloadTime":2.4,"Accuracy":1.15,"AimTime":1.8,"TurretArmorRear":40,"HullArmorFront":84,"HullArmorSide":55,"HullArmorRear":37},"Balanced":{"HP":330,"ShellsCount":100,"DamageMin":86.4,"DamageMax":129.6,"Penetration":132,"ReloadTime":2.4,"Accuracy":1.1,"AimTime":1.7,"TurretArmorRear":41,"HullArmorFront":85,"HullArmorSide":56,"HullArmorRear":38}},
          {"Code":"nv_l3_colossus_harrow","Level":3,"Class":2,"Original":{"HP":420,"ShellsCount":62,"Penetration":142,"ReloadTime":3.3,"Accuracy":1.75,"AimTime":2.65,"TurretArmorFront":120,"TurretArmorSide":78,"TurretArmorRear":53,"HullArmorFront":108,"HullArmorSide":70,"HullArmorRear":48},"Balanced":{"HP":435,"ShellsCount":65,"Penetration":145,"ReloadTime":3.2,"Accuracy":1.65,"AimTime":2.5,"TurretArmorFront":125,"TurretArmorSide":81,"TurretArmorRear":55,"HullArmorFront":113,"HullArmorSide":73,"HullArmorRear":50}},
          {"Code":"nv_l3_colossus_oblivion","Level":3,"Class":2,"Original":{"HP":470,"ShellSpeed":99,"ShellsCount":55,"DamageMin":129.6,"DamageMax":194.4,"Penetration":148,"ViewRange":88,"Accuracy":1.9,"AimTime":2.9,"TurretArmorFront":136,"TurretArmorSide":88,"TurretArmorRear":60,"HullArmorFront":122,"HullArmorSide":79,"HullArmorRear":54},"Balanced":{"HP":475,"ShellSpeed":102,"ShellsCount":58,"DamageMin":138.24,"DamageMax":207.36,"Penetration":152,"ViewRange":90,"Accuracy":1.85,"AimTime":2.8,"TurretArmorFront":140,"TurretArmorSide":90,"TurretArmorRear":62,"HullArmorFront":127,"HullArmorSide":82,"HullArmorRear":56}},
          {"Code":"nv_l4_colossus_eclipse","Level":4,"Class":2,"Original":{"HP":575,"ShellSpeed":93,"ShellsCount":50,"DamageMin":151.2,"DamageMax":226.8,"Penetration":158,"ViewRange":92,"Accuracy":2,"AimTime":3.05,"Speed":1.2,"Acceleration":0.26,"TraverseAcceleration":26,"TraverseSpeed":50,"TurretTraverseSpeed":44,"TurretArmorFront":160,"TurretArmorSide":103,"TurretArmorRear":70,"HullArmorFront":143,"HullArmorSide":94,"HullArmorRear":64},"Balanced":{"HP":590,"ShellSpeed":96,"ShellsCount":54,"DamageMin":164.16,"DamageMax":246.24,"Penetration":168,"ViewRange":94,"Accuracy":1.85,"AimTime":3,"Speed":1.25,"Acceleration":0.27,"TraverseAcceleration":27,"TraverseSpeed":51,"TurretTraverseSpeed":45,"TurretArmorFront":173,"TurretArmorSide":111,"TurretArmorRear":76,"HullArmorFront":156,"HullArmorSide":102,"HullArmorRear":70}}
        ]
        """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ApplyBalance(migrationBuilder, reverse: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ApplyBalance(migrationBuilder, reverse: true);
        }

        private static void ApplyBalance(MigrationBuilder migrationBuilder, bool reverse)
        {
            var expectedColumn = reverse ? "\"Balanced\"" : "\"Original\"";
            var targetColumn = reverse ? "\"Original\"" : "\"Balanced\"";
            var sql = $"""
                CREATE TEMP TABLE "_RavHiddenRobotBalance" ON COMMIT DROP AS
                SELECT *
                FROM jsonb_to_recordset('{BalanceRowsJson.Replace("'", "''", StringComparison.Ordinal)}'::jsonb)
                    AS entry("Code" text, "Level" integer, "Class" integer, "Original" jsonb, "Balanced" jsonb);

                DO $$
                BEGIN
                    IF (SELECT COUNT(*) FROM "_RavHiddenRobotBalance") <> 13
                       OR (SELECT COUNT(*) FROM "Vehicles" v JOIN "_RavHiddenRobotBalance" b ON b."Code" = v."Code") <> 13 THEN
                        RAISE EXCEPTION 'Hidden robot catalog differs from the reviewed 13-code balance set; refusing update.';
                    END IF;
                    IF EXISTS (
                        SELECT 1
                        FROM "_RavHiddenRobotBalance" b
                        JOIN "Vehicles" v ON v."Code" = b."Code"
                        WHERE v."Level" <> b."Level" OR v."Class" <> b."Class" OR v."IsVisible" IS DISTINCT FROM FALSE
                    ) THEN
                        RAISE EXCEPTION 'Hidden robot level, class, or visibility changed; refusing balance update.';
                    END IF;
                    IF EXISTS (
                        SELECT 1
                        FROM "_RavHiddenRobotBalance" b
                        JOIN "Vehicles" v ON v."Code" = b."Code"
                        WHERE jsonb_build_object(
                            'HP', v."HP", 'ShellSpeed', v."ShellSpeed", 'ShellsCount', v."ShellsCount",
                            'DamageMin', v."DamageMin", 'DamageMax', v."DamageMax", 'Penetration', v."Penetration",
                            'ViewRange', v."ViewRange", 'ReloadTime', v."ReloadTime", 'Accuracy', v."Accuracy",
                            'AimTime', v."AimTime", 'Speed', v."Speed", 'Acceleration', v."Acceleration",
                            'EnginePower', v."EnginePower", 'TraverseAcceleration', v."TraverseAcceleration",
                            'TraverseSpeed', v."TraverseSpeed", 'TurretTraverseSpeed', v."TurretTraverseSpeed",
                            'TurretArmorFront', v."TurretArmorFront", 'TurretArmorSide', v."TurretArmorSide",
                            'TurretArmorRear', v."TurretArmorRear", 'HullArmorFront', v."HullArmorFront",
                            'HullArmorSide', v."HullArmorSide", 'HullArmorRear', v."HullArmorRear"
                        ) @> b.{expectedColumn} IS NOT TRUE
                    ) THEN
                        RAISE EXCEPTION 'Hidden robot stats differ from the expected pre-balance state; refusing update.';
                    END IF;
                END $$;

                UPDATE "Vehicles" v
                SET "HP" = COALESCE((b.{targetColumn} ->> 'HP')::integer, v."HP"),
                    "ShellSpeed" = COALESCE((b.{targetColumn} ->> 'ShellSpeed')::real, v."ShellSpeed"),
                    "ShellsCount" = COALESCE((b.{targetColumn} ->> 'ShellsCount')::integer, v."ShellsCount"),
                    "DamageMin" = COALESCE((b.{targetColumn} ->> 'DamageMin')::real, v."DamageMin"),
                    "DamageMax" = COALESCE((b.{targetColumn} ->> 'DamageMax')::real, v."DamageMax"),
                    "Penetration" = COALESCE((b.{targetColumn} ->> 'Penetration')::integer, v."Penetration"),
                    "ViewRange" = COALESCE((b.{targetColumn} ->> 'ViewRange')::real, v."ViewRange"),
                    "ReloadTime" = COALESCE((b.{targetColumn} ->> 'ReloadTime')::real, v."ReloadTime"),
                    "Accuracy" = COALESCE((b.{targetColumn} ->> 'Accuracy')::real, v."Accuracy"),
                    "AimTime" = COALESCE((b.{targetColumn} ->> 'AimTime')::real, v."AimTime"),
                    "Speed" = COALESCE((b.{targetColumn} ->> 'Speed')::real, v."Speed"),
                    "Acceleration" = COALESCE((b.{targetColumn} ->> 'Acceleration')::real, v."Acceleration"),
                    "TraverseAcceleration" = COALESCE((b.{targetColumn} ->> 'TraverseAcceleration')::real, v."TraverseAcceleration"),
                    "TraverseSpeed" = COALESCE((b.{targetColumn} ->> 'TraverseSpeed')::real, v."TraverseSpeed"),
                    "TurretTraverseSpeed" = COALESCE((b.{targetColumn} ->> 'TurretTraverseSpeed')::real, v."TurretTraverseSpeed"),
                    "TurretArmorFront" = COALESCE((b.{targetColumn} ->> 'TurretArmorFront')::integer, v."TurretArmorFront"),
                    "TurretArmorSide" = COALESCE((b.{targetColumn} ->> 'TurretArmorSide')::integer, v."TurretArmorSide"),
                    "TurretArmorRear" = COALESCE((b.{targetColumn} ->> 'TurretArmorRear')::integer, v."TurretArmorRear"),
                    "HullArmorFront" = COALESCE((b.{targetColumn} ->> 'HullArmorFront')::integer, v."HullArmorFront"),
                    "HullArmorSide" = COALESCE((b.{targetColumn} ->> 'HullArmorSide')::integer, v."HullArmorSide"),
                    "HullArmorRear" = COALESCE((b.{targetColumn} ->> 'HullArmorRear')::integer, v."HullArmorRear")
                FROM "_RavHiddenRobotBalance" b
                WHERE v."Code" = b."Code";
                """;

            migrationBuilder.Sql(sql);
        }
    }
}
