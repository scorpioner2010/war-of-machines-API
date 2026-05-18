using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleViewRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "ViewRange",
                table: "Vehicles",
                type: "real",
                nullable: false,
                defaultValue: 100f);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehicles_ViewRange_Positive",
                table: "Vehicles",
                sql: "\"ViewRange\" > 0");

            migrationBuilder.Sql(@"
UPDATE ""Vehicles""
SET ""ViewRange"" = CASE ""Code""
    WHEN 'ia_l1_starter' THEN 105
    WHEN 'ia_l2_scout' THEN 125
    WHEN 'ia_l2_guardian' THEN 100
    WHEN 'ia_l2_colossus' THEN 85
    WHEN 'nv_l1_starter' THEN 110
    WHEN 'nv_l2_scout' THEN 130
    WHEN 'nv_l2_guardian' THEN 105
    WHEN 'nv_l2_colossus' THEN 90
    ELSE 100
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehicles_ViewRange_Positive",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ViewRange",
                table: "Vehicles");
        }
    }
}
