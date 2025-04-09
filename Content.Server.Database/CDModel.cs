// File to store as much CD related database things outside of Model.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

public static class CDModel
{
    /// <summary>
    /// Stores CD Character data separately from the main Profile. This is done to work around a bug
    /// in EFCore migrations.
    /// <p />
    /// There is no way of forcing a dependent table to exist in EFCore (according to MS).
    /// You must always account for the possibility of this table not existing.
    /// </summary>
    public class CDProfile
    {
        public int Id { get; set; }

        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        public float Height { get; set; } = 1f;

        [Column("character_records", TypeName = "jsonb")]
        public JsonDocument? CharacterRecords { get; set; }

        public List<CharacterRecordEntry> CharacterRecordEntries { get; set; } = new();

    }
    public enum DbRecordEntryType : byte
    {
         Medical = 0, Security = 1, Employment = 2
    }

    [Table("cd_character_record_entries"), Index(nameof(Id))]
    public sealed class CharacterRecordEntry
    {
        public int Id { get; set;  }

        public string Title { get; set; } = null!;

        public string Involved { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DbRecordEntryType Type { get; set; }

        public int CDProfileId { get; set; }
        public CDProfile CDProfile { get; set; } = null!;
    }
}
