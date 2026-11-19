using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarOfMachines.Migrations
{
    /// <inheritdoc />
    public partial class SecurityHardeningFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchParticipants_MatchId",
                table: "MatchParticipants");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalized",
                table: "MatchParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DedicatedServerId",
                table: "Matches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConnectionTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedeemedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DedicatedServers",
                columns: table => new
                {
                    ServerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CredentialHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Region = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    PlayersOnline = table.Column<int>(type: "integer", nullable: false),
                    ActiveMatches = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastHeartbeatUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsRegistered = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DedicatedServers", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUsedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchParticipants_MatchId_UserId",
                table: "MatchParticipants",
                columns: new[] { "MatchId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_DedicatedServerId_EndedAt",
                table: "Matches",
                columns: new[] { "DedicatedServerId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionTickets_ServerId_ExpiresAtUtc_RedeemedAtUtc",
                table: "ConnectionTickets",
                columns: new[] { "ServerId", "ExpiresAtUtc", "RedeemedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionTickets_TokenHash",
                table: "ConnectionTickets",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DedicatedServers_LastHeartbeatUtc",
                table: "DedicatedServers",
                column: "LastHeartbeatUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RefreshTokenHash",
                table: "UserSessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_RevokedAtUtc",
                table: "UserSessions",
                columns: new[] { "UserId", "RevokedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionTickets");

            migrationBuilder.DropTable(
                name: "DedicatedServers");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_MatchParticipants_MatchId_UserId",
                table: "MatchParticipants");

            migrationBuilder.DropIndex(
                name: "IX_Matches_DedicatedServerId_EndedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsFinalized",
                table: "MatchParticipants");

            migrationBuilder.DropColumn(
                name: "DedicatedServerId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Matches");

            migrationBuilder.CreateIndex(
                name: "IX_MatchParticipants_MatchId",
                table: "MatchParticipants",
                column: "MatchId");
        }
    }
}
