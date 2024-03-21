using Content.Shared.Audio;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Content.Shared.DeltaV.Gavel;
using Robust.Shared.Audio.Systems;

namespace Content.Server.DeltaV.EntitySystems;

public sealed class GavelBlockSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GavelBlockComponent, AfterInteractUsingEvent>(OnInteract);
    }

    private void OnInteract(Entity<GavelBlockComponent> entity, ref AfterInteractUsingEvent args) // We make this component
    {
        if (!args.CanReach || args.Target == null ||
            !_tagSystem.HasTag(args.Used, GavelBlockComponent.GavelTag)) // We make this component and tag
        {
            return;
        }

        _audioSystem.PlayPvs(entity.Comp.GavelSound, entity); // Assuming we have gavel sound
    }
}
