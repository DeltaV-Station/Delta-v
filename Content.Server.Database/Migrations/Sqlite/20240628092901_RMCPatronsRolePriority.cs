using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DeltaVPatronsRolePriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "discord_role",
                table: "patreon_tiers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "patreon_tiers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "priority",
                table: "patreon_tiers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_patreon_tiers_discord_role",
                table: "patreon_tiers",
                column: "discord_role",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patreon_tiers_discord_role",
                table: "patreon_tiers");

            migrationBuilder.DropColumn(
                name: "discord_role",
                table: "patreon_tiers");

            migrationBuilder.DropColumn(
                name: "name",
                table: "patreon_tiers");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "patreon_tiers");
        }
    }
}
