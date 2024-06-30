using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DeltaVPatronsCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_linking_codes",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    code = table.Column<Guid>(type: "TEXT", nullable: false),
                    creation_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_linking_codes", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_discord_linking_codes_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discord_linking_codes_code",
                table: "discord_linking_codes",
                column: "code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_linking_codes");
        }
    }
}
