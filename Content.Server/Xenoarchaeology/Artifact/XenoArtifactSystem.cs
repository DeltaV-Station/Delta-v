using System.Text; // DeltaV
using Content.Shared.Administration.Logs; // DeltaV
using Content.Shared.Cargo;
using Content.Shared.Database; // DeltaV
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.Components;

namespace Content.Server.Xenoarchaeology.Artifact;

/// <inheritdoc cref="SharedXenoArtifactSystem"/>
public sealed partial class XenoArtifactSystem : SharedXenoArtifactSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!; // DeltaV - research statistics admin logs

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoArtifactComponent, MapInitEvent>(OnArtifactMapInit);
        SubscribeLocalEvent<XenoArtifactComponent, PriceCalculationEvent>(OnCalculatePrice);
    }

    private void OnArtifactMapInit(Entity<XenoArtifactComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.IsGenerationRequired)
            GenerateArtifactStructure(ent);

        // DeltaV - start of research statistics admin logs
        foreach (var node in GetAllNodes(ent))
        {
            var effectStatusString = node.Comp.LockedEffectTipHidden ? "Hidden" : node.Comp.LockedEffectTipVague ? "Vague" : "Specific";

            var triggerStr = new StringBuilder();
            var predecessors = GetPredecessorNodes((ent, ent), node);
            triggerStr.Append(predecessors.Count + 1);
            triggerStr.Append(" triggers: ");
            triggerStr.Append(node.Comp.TriggerTip ?? "Unknown");
            foreach (var predecessor in predecessors)
            {
                triggerStr.Append(",");
                triggerStr.Append(predecessor.Comp.TriggerTip ?? "Unknown");
            }

            _adminLogger.Add(
                LogType.ArtifactDetails,
                LogImpact.Low,
                // note: effect type is already logged by ToPrettyString(node)
                $"{ToPrettyString(ent.Owner)} spawned with node {ToPrettyString(node)} with depth {node.Comp.Depth}; effect status {effectStatusString}; {triggerStr.ToString()}"
            );
        }
        // DeltaV - end of research statistics admin logs
    }

    private void OnCalculatePrice(Entity<XenoArtifactComponent> ent, ref PriceCalculationEvent args)
    {
        foreach (var node in GetAllNodes(ent))
        {
            if (node.Comp.Locked)
                continue;

            args.Price += node.Comp.ResearchValue * ent.Comp.PriceMultiplier;
        }
    }
}
