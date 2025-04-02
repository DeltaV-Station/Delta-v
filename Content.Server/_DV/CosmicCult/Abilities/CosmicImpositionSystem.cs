using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicImpositionSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicImposingComponent, BeforeDamageChangedEvent>(OnImpositionDamaged);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicImposition>(OnCosmicImposition);
    }

    private void OnCosmicImposition(Entity<CosmicCultComponent> uid, ref EventCosmicImposition args)
    {
        EnsureComp<CosmicImposingComponent>(uid, out var comp);
        Timer.Spawn(uid.Comp.CosmicImpositionDuration, () => RemComp(uid, comp));
        Spawn(uid.Comp.ImpositionVFX, Transform(uid).Coordinates);
        args.Handled = true;
        _audio.PlayPvs(uid.Comp.ImpositionSFX, uid, AudioParams.Default.WithVariation(0.05f));
        _cult.MalignEcho(uid);
    }

    private void OnImpositionDamaged(Entity<CosmicImposingComponent> uid, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
    }
}
