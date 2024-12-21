using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CDRecordJsonToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cd_character_record_entries",
                columns: table => new
                {
                    cd_character_record_entries_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    involved = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    cdprofile_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cd_character_record_entries", x => x.cd_character_record_entries_id);
                    table.ForeignKey(
                        name: "FK_cd_character_record_entries_cdprofile_cdprofile_id",
                        column: x => x.cdprofile_id,
                        principalTable: "cdprofile",
                        principalColumn: "cdprofile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cd_character_record_entries_cd_character_record_entries_id",
                table: "cd_character_record_entries",
                column: "cd_character_record_entries_id");

            migrationBuilder.CreateIndex(
                name: "IX_cd_character_record_entries_cdprofile_id",
                table: "cd_character_record_entries",
                column: "cdprofile_id");


            // See the comment in the SQLite version.
            // This is not exactly the same because Postgres and SQLite have different syntax for JSON access.
            // WHY IS THERE NOT A STANDARD WAY OF DOING THIS!!!
            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        jsonb_array_elements.value ->> 'Title', jsonb_array_elements.value ->> 'Involved', jsonb_array_elements.value ->> 'Description',
                        {(int)CDModel.DbRecordEntryType.Medical}, cdprofile_id
                    FROM
                        cdprofile, jsonb_array_elements(character_records -> 'MedicalEntries');
                """);

            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        jsonb_array_elements.value ->> 'Title', jsonb_array_elements.value ->> 'Involved', jsonb_array_elements.value ->> 'Description',
                        {(int)CDModel.DbRecordEntryType.Security}, cdprofile_id
                    FROM
                        cdprofile, jsonb_array_elements(character_records -> 'SecurityEntries')
                """);

            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        jsonb_array_elements.value ->> 'Title', jsonb_array_elements.value ->> 'Involved', jsonb_array_elements.value ->> 'Description',
                        {(int)CDModel.DbRecordEntryType.Employment}, cdprofile_id
                    FROM
                        cdprofile, jsonb_array_elements(character_records -> 'EmploymentEntries')
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cd_character_record_entries");
        }
    }
}
