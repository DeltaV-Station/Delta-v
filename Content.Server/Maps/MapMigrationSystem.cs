using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed class MapMigrationSystem : EntitySystem
{
#pragma warning disable CS0414
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
#pragma warning restore CS0414
    [Dependency] private readonly IResourceManager _resMan = default!;

    private const string MigrationDir = "/Migrations/"; // DeltaV - dir instead of a single file

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        if (!TryReadFile(out var mappings))
            return;

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var mapping in mappings) // DeltaV - iterate each mapping file
        {
            foreach (var node in mapping.Children.Values)
            {
                var newId = ((ValueDataNode) node).Value;
                if (!string.IsNullOrEmpty(newId) && newId != "null")
                    DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId),
                        $"{newId} is not an entity prototype.");
            }
        }
#endif
    }

    private bool TryReadFile([NotNullWhen(true)] out List<MappingDataNode>? mappings) // DeltaV - changed to a list
    {
        mappings = null;

        // Begin DeltaV Changes - rewrote single path to finding files in a directory
        var files = _resMan.ContentFindFiles(MigrationDir)
            .Where(f => f.ToString().EndsWith(".yml"))
            .ToList();

        if (files.Count == 0)
            return false;

        foreach (var file in files)
        {
            if (!_resMan.TryContentFileRead(file, out var stream))
                continue;

            using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
            var documents = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

            if (documents == null)
                continue;

            mappings = mappings ?? new List<MappingDataNode>();
            mappings.Add((MappingDataNode)documents.Root);
        }

        return mappings != null && mappings.Count > 0;
        // End DeltaV Changes
    }

    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
        if (!TryReadFile(out var mappings))
            return;

        foreach (var mapping in mappings) // DeltaV - iterate a list of mappings
        {
            foreach (var (key, value) in mapping)
            {
                if (value is not ValueDataNode valueNode)
                    continue;

                if (string.IsNullOrWhiteSpace(valueNode.Value) || valueNode.Value == "null")
                    ev.DeletedPrototypes.Add(key);
                else
                    ev.RenamedPrototypes.Add(key, valueNode.Value);
            }
        }
    }
}
