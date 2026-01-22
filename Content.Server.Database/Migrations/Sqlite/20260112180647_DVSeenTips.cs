using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DVSeenTips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dv_seen_tips",
                columns: table => new
                {
                    dv_seen_tips_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    tip_proto_id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    dismissed_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dv_seen_tips", x => x.dv_seen_tips_id);
                    table.ForeignKey(
                        name: "FK_dv_seen_tips_player_player_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dv_seen_tips_player_user_id_tip_proto_id",
                table: "dv_seen_tips",
                columns: new[] { "player_user_id", "tip_proto_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dv_seen_tips");
        }
    }
}
