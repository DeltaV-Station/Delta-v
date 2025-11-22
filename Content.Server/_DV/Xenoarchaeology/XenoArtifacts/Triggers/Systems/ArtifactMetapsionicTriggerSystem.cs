using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Triggers.Components;
﻿using Content.Server.Nyanotrasen.StationEvents.Events;
using Content.Server.Xenoarchaeology.Artifact;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Content.Shared.Xenoarchaeology.Artifact.XAT;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactMetapsionicTriggerSystem : BaseXATSystem<ArtifactMetapsionicTriggerComponent>
{
    [Dependency] private readonly XenoArtifactSystem _artifact = default!;

    private EntityQuery<XenoArtifactComponent> _xenoArtifactQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xenoArtifactQuery = GetEntityQuery<XenoArtifactComponent>();

        SubscribeLocalEvent<ArtifactMetapsionicTriggerComponent, PsionicPowerDetectedEvent>(OnPowerDetected);

        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
    }

    private void OnPowerDetected(Entity<ArtifactMetapsionicTriggerComponent> ent, ref PsionicPowerDetectedEvent args)
    {
        if (!TryComp<XenoArtifactNodeComponent>(ent, out var node))
            return;

        if (node.Attached == null)
            return;

        var artifact = _xenoArtifactQuery.Get(GetEntity(node.Attached.Value));

        if (!CanTrigger(artifact, (ent.Owner, node)))
            return;

        Trigger(artifact, (ent.Owner, ent.Comp, node));
    }

    private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<ArtifactMetapsionicTriggerComponent, XenoArtifactNodeComponent>();
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
