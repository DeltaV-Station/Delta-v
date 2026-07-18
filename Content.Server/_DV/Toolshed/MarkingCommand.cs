using System.Diagnostics;
using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Utility;

namespace Content.Server._DV.Toolshed;

/// <summary>
/// Toolshed commands for reading and manipulating the markings of a humanoid (an entity with a
/// <see cref="VisualBodyComponent"/>). The piped input is the humanoid to operate on.
/// </summary>
/// <example><code>
/// ent 12345 marking:add HumanHairAfro
/// ent 12345 marking:add HumanHairAfro #ff0000
/// ent 12345 marking:remove HumanHairAfro
/// ent 12345 marking:get
/// </code></example>
[ToolshedCommand(Name = "marking"), AdminCommand(AdminFlags.Fun)]
public sealed class MarkingCommand : ToolshedCommand
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private SharedVisualBodySystem? _visualBody;

    // Note: the color is optional and defaults to the transparent default(Color), which we treat as "unspecified"
    // and fall back to the marking prototype's own colors. Toolshed has no parser for Nullable<Color>, so a real
    // nullable argument can't be parsed from a literal - this sentinel is the clean way to make it optional.
    [CommandImplementation("add")]
    public EntityUid Add([PipedArgument] EntityUid ent, ProtoId<MarkingPrototype> marking, IInvocationContext ctx, Color color = default)
    {
        _visualBody ??= GetSys<SharedVisualBodySystem>();

        var proto = _prototype.Index(marking);
        var layer = proto.BodyPart;

        if (!_visualBody.TryGatherMarkingsData(ent, null, out _, out var markingData, out var applied))
        {
            ctx.ReportError(new NotAHumanoidError(ent));
            return ent;
        }

        if (!TryGetCategoryForLayer(markingData, layer, out var category))
        {
            ctx.ReportError(new NoOrganForMarkingError(ent, marking, layer));
            return ent;
        }

        var newMarking = proto.AsMarking();
        if (color != default)
            newMarking = newMarking.WithColor(color);

        var markings = applied.GetValueOrDefault(category)?.GetValueOrDefault(layer)?.ToList() ?? new List<Marking>();
        markings.Add(newMarking);

        _visualBody.ApplyMarkings(ent, new()
        {
            [category] = new() { [layer] = markings },
        });

        return ent;
    }

    [CommandImplementation("remove")]
    public EntityUid Remove([PipedArgument] EntityUid ent, ProtoId<MarkingPrototype> marking, IInvocationContext ctx)
    {
        _visualBody ??= GetSys<SharedVisualBodySystem>();

        var proto = _prototype.Index(marking);
        var layer = proto.BodyPart;

        if (!_visualBody.TryGatherMarkingsData(ent, null, out _, out var markingData, out var applied))
        {
            ctx.ReportError(new NotAHumanoidError(ent));
            return ent;
        }

        if (!TryGetCategoryForLayer(markingData, layer, out var category))
        {
            ctx.ReportError(new NoOrganForMarkingError(ent, marking, layer));
            return ent;
        }

        var markings = applied.GetValueOrDefault(category)?.GetValueOrDefault(layer)?.ToList() ?? new List<Marking>();
        markings.RemoveAll(m => m.MarkingId == marking);

        _visualBody.ApplyMarkings(ent, new()
        {
            [category] = new() { [layer] = markings },
        });

        return ent;
    }

    [CommandImplementation("get")]
    public IEnumerable<Marking> Get([PipedArgument] EntityUid ent, IInvocationContext ctx)
    {
        _visualBody ??= GetSys<SharedVisualBodySystem>();

        if (!_visualBody.TryGatherMarkingsData(ent, null, out _, out _, out var applied))
        {
            ctx.ReportError(new NotAHumanoidError(ent));
            yield break;
        }

        foreach (var layers in applied.Values)
        {
            foreach (var markings in layers.Values)
            {
                foreach (var marking in markings)
                {
                    yield return marking;
                }
            }
        }
    }

    /// <summary>
    /// Finds the organ category that owns the given humanoid visual layer.
    /// </summary>
    private static bool TryGetCategoryForLayer(Dictionary<ProtoId<OrganCategoryPrototype>, OrganMarkingData> markingData, HumanoidVisualLayers layer, out ProtoId<OrganCategoryPrototype> category)
    {
        foreach (var (cat, data) in markingData)
        {
            if (data.Layers.Contains(layer))
            {
                category = cat;
                return true;
            }
        }

        category = default;
        return false;
    }
}

public sealed class NotAHumanoidError(EntityUid ent) : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted($"Entity {ent} is not a humanoid (has no visual body).");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}

public sealed class NoOrganForMarkingError(EntityUid ent, string marking, HumanoidVisualLayers layer) : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted($"Entity {ent} has no organ that accepts marking '{marking}' (layer {layer}).");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}
