using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.StationEvents.Events;
using Content.Shared._DV.Xenoarcheology.XenoArtifacts.Triggers.Components;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Content.Shared.Xenoarchaeology.Artifact.XAT;

namespace Content.Shared._DV.Xenoarcheology.XenoArtifacts.Triggers.Systems;

public sealed class XATPsionicUsageSystem : BaseXATSystem<XATPsionicUsageComponent>
{
    private EntityQuery<XenoArtifactComponent> _xenoArtifactQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xenoArtifactQuery = GetEntityQuery<XenoArtifactComponent>();

        XATSubscribeDirectEvent<PsionicPowerDetectedEvent>(OnPowerDetected);
        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
    }

    private void OnPowerDetected(Entity<XenoArtifactComponent> artifact, Entity<XATPsionicUsageComponent, XenoArtifactNodeComponent> node, ref PsionicPowerDetectedEvent args)
    {
        Trigger(artifact, node);
    }

    private void OnGlimmerEventEnded(ref GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<XATPsionicUsageComponent, XenoArtifactNodeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var node))
        {
            if (node.Attached == null)
                continue;

            var artifact = _xenoArtifactQuery.Get(GetEntity(node.Attached.Value));

            if (!CanTrigger(artifact, (uid, node)))
                continue;

            Trigger(artifact, (uid, comp, node));
        }
    }
}
