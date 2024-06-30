using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class DiscordLinkedAccountLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_linked_accounts_logs",
                columns: table => new
                {
                    discord_linked_accounts_logs_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discord_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_linked_accounts_logs", x => x.discord_linked_accounts_logs_id);
                    table.ForeignKey(
                        name: "FK_discord_linked_accounts_logs_player__player_id1",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_discord_linked_accounts_logs_discord_accounts_discord_id",
                        column: x => x.discord_id,
                        principalTable: "discord_accounts",
                        principalColumn: "discord_accounts_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discord_linked_accounts_logs_at",
                table: "discord_linked_accounts_logs",
                column: "at");

            migrationBuilder.CreateIndex(
                name: "IX_discord_linked_accounts_logs_discord_id",
                table: "discord_linked_accounts_logs",
                column: "discord_id");

            migrationBuilder.CreateIndex(
                name: "IX_discord_linked_accounts_logs_player_id",
                table: "discord_linked_accounts_logs",
                column: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_linked_accounts_logs");
        }
    }
}
