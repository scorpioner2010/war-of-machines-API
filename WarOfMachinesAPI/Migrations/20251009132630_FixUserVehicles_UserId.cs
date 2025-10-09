using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class FixUserVehicles_UserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "UserVehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVehicles_PlayerId",
                table: "UserVehicles",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserVehicles_Players_PlayerId",
                table: "UserVehicles",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserVehicles_Players_PlayerId",
                table: "UserVehicles");

            migrationBuilder.DropIndex(
                name: "IX_UserVehicles_PlayerId",
                table: "UserVehicles");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "UserVehicles");
        }
    }
}
