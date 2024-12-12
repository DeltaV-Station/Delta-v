using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
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
                    cd_character_record_entries_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    involved = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    type = table.Column<byte>(type: "INTEGER", nullable: false),
                    cdprofile_id = table.Column<int>(type: "INTEGER", nullable: false)
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

            // Manually copy over entries to their table, entries will be erased from the JSON blob the next time
            // the character is saved. This was literal pain to make.
            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        json_each.value ->> '$.Title', json_each.value ->> '$.Involved', json_each.value ->> '$.Description',
                        {(int)CDModel.DbRecordEntryType.Medical}, cdprofile_id
                    FROM
                        cdprofile, json_each(character_records, '$.MedicalEntries')
                """);

            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        json_each.value ->> '$.Title', json_each.value ->> '$.Involved', json_each.value ->> '$.Description',
                        {(int)CDModel.DbRecordEntryType.Security}, cdprofile_id
                    FROM
                        cdprofile, json_each(character_records, '$.SecurityEntries')
                """);

            migrationBuilder.Sql($"""
                INSERT INTO cd_character_record_entries (title, involved, description, type, cdprofile_id)
                    SELECT
                        json_each.value ->> '$.Title', json_each.value ->> '$.Involved', json_each.value ->> '$.Description',
                        {(int)CDModel.DbRecordEntryType.Employment}, cdprofile_id
                    FROM
                        cdprofile, json_each(character_records, '$.EmploymentEntries')
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
