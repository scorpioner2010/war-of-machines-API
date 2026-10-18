using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WarOfMachines.Data;

#nullable disable

namespace WarOfMachines.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261018120000_AddMatchParticipantRewardAmounts")]
    public partial class AddMatchParticipantRewardAmounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoltsEarned",
                table: "MatchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeXpEarned",
                table: "MatchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoltsEarned",
                table: "MatchParticipants");

            migrationBuilder.DropColumn(
                name: "FreeXpEarned",
                table: "MatchParticipants");
        }
    }
}
