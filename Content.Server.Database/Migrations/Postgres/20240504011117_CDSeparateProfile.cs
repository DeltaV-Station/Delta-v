using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CDSeparateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cdprofile",
                columns: table => new
                {
                    cdprofile_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    profile_id = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<float>(type: "real", nullable: false),
                    character_records = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdprofile", x => x.cdprofile_id);
                    table.ForeignKey(
                        name: "FK_cdprofile_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cdprofile_profile_id",
                table: "cdprofile",
                column: "profile_id",
                unique: true);

            migrationBuilder.Sql(
                "INSERT INTO cdprofile (profile_id, height, character_records) SELECT profile_id, height, cd_character_records FROM profile");

            migrationBuilder.DropColumn(
                name: "cd_character_records",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "height",
                table: "profile");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cdprofile");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "cd_character_records",
                table: "profile",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "height",
                table: "profile",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
