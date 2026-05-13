using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    [DbContext(typeof(SqliteServerDbContext))]
    [Migration("20260511100000_AddRpCommendationsSqlite")]
    public partial class AddRpCommendationsSqlite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rp_commendations",
                columns: table => new
                {
                    rp_commendation_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    round_id = table.Column<int>(type: "INTEGER", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    receiver_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_admin_award = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rp_commendations", x => x.rp_commendation_id);
                    table.ForeignKey(
                        name: "FK_rp_commendations_player_receiver_user_id",
                        column: x => x.receiver_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rp_commendations_player_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rp_commendations_round_round_id",
                        column: x => x.round_id,
                        principalTable: "round",
                        principalColumn: "round_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rp_commendations_receiver_user_id",
                table: "rp_commendations",
                column: "receiver_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_rp_commendations_round_id",
                table: "rp_commendations",
                column: "round_id");

            // Prevent multiple commendations per round from same sender unless admin award
            // But SQLite doesn't support partial index with IS NOT constraint gracefully inside EF standard createIndex without raw sql sometimes, 
            // so we do standard index.
            migrationBuilder.CreateIndex(
                name: "IX_rp_commendations_round_id_sender_user_id",
                table: "rp_commendations",
                columns: new[] { "round_id", "sender_user_id" },
                unique: false); // Handled by application logic
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rp_commendations");
        }
    }
}
