using Content.Server.DeltaV.Xenoarchaeology.XenoArtifacts.Triggers.Components;
﻿using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactMetapsionicTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArtifactMetapsionicTriggerComponent, PsionicPowerDetectedEvent>(OnPowerDetected);
    }

    private void OnPowerDetected(Entity<ArtifactMetapsionicTriggerComponent> ent, ref PsionicPowerDetectedEvent args)
    {
        _artifact.TryActivateArtifact(ent);
    }
}
