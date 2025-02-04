using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Triggers.Components;
﻿using Content.Server.Nyanotrasen.StationEvents.Events;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactMetapsionicTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtifactMetapsionicTriggerComponent, PsionicPowerDetectedEvent>(OnPowerDetected);

        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
    }

    private void OnPowerDetected(Entity<ArtifactMetapsionicTriggerComponent> ent, ref PsionicPowerDetectedEvent args)
    {
        _artifact.TryActivateArtifact(ent);
    }

    private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<ArtifactMetapsionicTriggerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _artifact.TryActivateArtifact(uid);
        }
    }
}
