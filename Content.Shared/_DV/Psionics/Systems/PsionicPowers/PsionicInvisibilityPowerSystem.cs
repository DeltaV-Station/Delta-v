using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class PsionicInvisibilityPowerSystem : BasePsionicPowerSystem<PsionicInvisibilityPowerComponent, PsionicInvisibilityPowerActionEvent>
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DispelledEvent>(OnDispelled);
    }

    protected override void OnPowerUsed(Entity<PsionicInvisibilityPowerComponent> psionic, ref PsionicInvisibilityPowerActionEvent args)
    {
        ToggleInvisibility(args.Performer);
        LogPowerUsed(psionic, args.Performer);
    }

    private void OnInit(Entity<PsionicInvisibilityUsedComponent> invisible, ref MapInitEvent args)
    {
        EnsureComp<PsionicallyInvisibleComponent>(invisible);
        EnsureComp<PacifiedComponent>(invisible);
        var stealth = EnsureComp<StealthComponent>(invisible);
        _stealth.SetVisibility(invisible, 0.66f, stealth);
        _audio.PlayPredicted(invisible.Comp.Sound, invisible, invisible);
        Psionic.SetCanSeePsionicInvisiblity(invisible, true);
    }

    private void OnShutdown(Entity<PsionicInvisibilityUsedComponent> invisible, ref ComponentShutdown args)
    {
        if (Terminating(invisible))
            return;

        RemComp<PsionicallyInvisibleComponent>(invisible);
        RemComp<PacifiedComponent>(invisible);
        RemComp<StealthComponent>(invisible);
        _audio.PlayPvs(invisible.Comp.Sound, invisible);
        Psionic.SetCanSeePsionicInvisiblity(invisible, false);

        _stunSystem.TryAddParalyzeDuration(invisible, TimeSpan.FromSeconds(4));
    }

    private void OnDamageChanged(Entity<PsionicInvisibilityUsedComponent> invisible, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        ToggleInvisibility(invisible);
    }

    private void OnDispelled(Entity<PsionicInvisibilityUsedComponent> invisible, ref DispelledEvent args)
    {
        ToggleInvisibility(invisible);
    }

    public void ToggleInvisibility(EntityUid invisible)
    {
        if (!RemComp<PsionicInvisibilityUsedComponent>(invisible))
            EnsureComp<PsionicInvisibilityUsedComponent>(invisible);
    }
}
