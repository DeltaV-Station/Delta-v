using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Triggers.Components;
﻿using Content.Server.Nyanotrasen.StationEvents.Events;
using Content.Server.Xenoarchaeology.Artifact;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Xenoarchaeology.Artifact.Components;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactMetapsionicTriggerSystem : EntitySystem
{
    [Dependency] private readonly XenoArtifactSystem _artifact = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtifactMetapsionicTriggerComponent, PsionicPowerDetectedEvent>(OnPowerDetected);

        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
    }

    private void OnPowerDetected(Entity<ArtifactMetapsionicTriggerComponent> ent, ref PsionicPowerDetectedEvent args)
    {
        if (!TryComp<XenoArtifactComponent>(ent, out var artifactComp))
            return;

        var artifactEntity = new Entity<XenoArtifactComponent>(ent, artifactComp);
        var coords = Transform(ent).Coordinates;

        _artifact.TryActivateXenoArtifact(
            artifactEntity,
            user: null,
            target: null,
            coordinates: coords
        );
    }

    private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<ArtifactMetapsionicTriggerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!TryComp<XenoArtifactComponent>(uid, out var artifactComp))
                continue;

            var artifactEntity = new Entity<XenoArtifactComponent>(uid, artifactComp);
            var coords = Transform(uid).Coordinates;

            _artifact.TryActivateXenoArtifact(
                artifactEntity,
                user: null,
                target: null,
                coordinates: coords
            );
        }
    }
}
