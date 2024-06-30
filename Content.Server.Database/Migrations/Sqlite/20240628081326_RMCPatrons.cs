using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DeltaVPatrons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_accounts",
                columns: table => new
                {
                    discord_accounts_id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_accounts", x => x.discord_accounts_id);
                });

            migrationBuilder.CreateTable(
                name: "discord_patron_tiers",
                columns: table => new
                {
                    discord_patron_tiers_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    show_on_credits = table.Column<bool>(type: "INTEGER", nullable: false),
                    named_items = table.Column<bool>(type: "INTEGER", nullable: false),
                    figurines = table.Column<bool>(type: "INTEGER", nullable: false),
                    lobby_message = table.Column<bool>(type: "INTEGER", nullable: false),
                    round_end_shoutout = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_patron_tiers", x => x.discord_patron_tiers_id);
                });

            migrationBuilder.CreateTable(
                name: "discord_linked_accounts",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    discord_id = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_linked_accounts", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_discord_linked_accounts_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_discord_linked_accounts_discord_accounts_discord_id",
                        column: x => x.discord_id,
                        principalTable: "discord_accounts",
                        principalColumn: "discord_accounts_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discord_patrons",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tier_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_patrons", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_discord_patrons_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_discord_patrons_discord_patron_tiers_tier_id",
                        column: x => x.tier_id,
                        principalTable: "discord_patron_tiers",
                        principalColumn: "discord_patron_tiers_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discord_linked_accounts_discord_id",
                table: "discord_linked_accounts",
                column: "discord_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discord_patrons_tier_id",
                table: "discord_patrons",
                column: "tier_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_linked_accounts");

            migrationBuilder.DropTable(
                name: "discord_patrons");

            migrationBuilder.DropTable(
                name: "discord_accounts");

            migrationBuilder.DropTable(
                name: "discord_patron_tiers");
        }
    }
}
