using Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server._DV.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class GlimmerArtifactSystem : EntitySystem
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlimmerArtifactComponent, XenoArtifactActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<GlimmerArtifactComponent> ent, ref XenoArtifactActivatedEvent args)
    {
        var range = ent.Comp.Range;
        var current = _glimmer.Glimmer;
        if (current < range.Min || current > range.Max)
            return;

        _glimmer.Glimmer += ent.Comp.Change;
    }
}
