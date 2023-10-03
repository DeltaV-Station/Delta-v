using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Utility;


namespace Content.Client.Changelog
{
    public sealed partial class ChangelogManager
    {
        [Dependency] private readonly IResourceManager _resource = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;

        public bool NewChangelogEntries { get; private set; }
        public DateTime LastReadTime { get; private set; } // Modified, see EOF
        public DateTime MaxTime { get; private set; } // Modified, see EOF

        public event Action? NewChangelogEntriesChanged;

        /// <summary>
        ///     Ran when the user opens ("read") the changelog,
        ///     stores the new ID to disk and clears <see cref="NewChangelogEntries"/>.
        /// </summary>
        /// <remarks>
        ///     <see cref="LastReadTime"/> is NOT cleared
        ///     since that's used in the changelog menu to show the "since you last read" bar.
        /// </remarks>
        public void SaveNewReadId()
        {
            NewChangelogEntries = false;
            NewChangelogEntriesChanged?.Invoke();

            using var sw = _resource.UserData.OpenWriteText(new ($"/changelog_last_seen_{_configManager.GetCVar(CCVars.ServerId)}_datetime")); // Modified, see EOF

            sw.Write(MaxTime.ToString("O")); // Modified, see EOF
        }

        public async void Initialize()
        {
            // Open changelog purely to compare to the last viewed date.
            var changelog = await LoadChangelog();

            if (changelog.Count == 0)
            {
                return;
            }

            MaxTime = changelog.Max(c => c.Time); // Modified, see EOF

            // Begin modified codeblock, see EOF
            var path = new ResPath($"/changelog_last_seen_{_configManager.GetCVar(CCVars.ServerId)}_datetime");
            if(_resource.UserData.TryReadAllText(path, out var lastReadTimeText))
            {
                if (Regex.IsMatch(lastReadTimeText,
                        @"^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$"))
                {
                    LastReadTime = DateTime.ParseExact(lastReadTimeText, "O", CultureInfo.InvariantCulture);
                }
            }

            NewChangelogEntries = LastReadTime < MaxTime;
            // End modified codeblock

            NewChangelogEntriesChanged?.Invoke();
        }

        // Begin modified codeblock, see EOF
        public async Task<List<ChangelogEntry>> LoadChangelog()
        {
            var paths = _resource.ContentFindFiles("/Changelog/")
                .Where(filePath => filePath.Extension == "yml")
                .ToArray();

            var result = new List<ChangelogEntry>();
            foreach (var path in paths)
            {
                var changelog = await LoadChangelogFile(path);
                result = result.Union(changelog).ToList();
            }
            return result.OrderBy(x => x.Time).ToList();
        }

        private Task<List<ChangelogEntry>> LoadChangelogFile(ResPath path) // end modified codeblock
        {
            return Task.Run(() =>
            {
                var yamlData = _resource.ContentFileReadYaml(path); // Modified, see EOF

                if (yamlData.Documents.Count == 0)
                    return new List<ChangelogEntry>();

                var node = (MappingDataNode)yamlData.Documents[0].RootNode.ToDataNode();
                return _serialization.Read<List<ChangelogEntry>>(node["Entries"], notNullableOverride: true);
            });
        }

        [DataDefinition]
        public sealed partial class ChangelogEntry : ISerializationHooks
        {
            [DataField("id")]
            public int Id { get; private set; }

            [DataField("author")]
            public string Author { get; private set; } = "";

            [DataField("time")] private string _time = default!;

            public DateTime Time { get; private set; }

            [DataField("changes")]
            public List<ChangelogChange> Changes { get; private set; } = default!;

            void ISerializationHooks.AfterDeserialization()
            {
                Time = DateTime.Parse(_time, null, DateTimeStyles.RoundtripKind);
            }
        }

        [DataDefinition]
        public sealed partial class ChangelogChange : ISerializationHooks
        {
            [DataField("type")]
            public ChangelogLineType Type { get; private set; }

            [DataField("message")]
            public string Message { get; private set; } = "";
        }

        public enum ChangelogLineType
        {
            Add,
            Remove,
            Fix,
            Tweak,
        }
    }
}


// This file was extensively modified to allow for datetime based changelogs instead of relying on IDs.
// This is because our IDs are much lower then Wizdens, and if we use their entries, the server will not properly show new changes
