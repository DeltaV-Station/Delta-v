using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class PsionicRegenerationPowerSystem : BasePsionicPowerSystem<PsionicRegenerationPowerComponent, PsionicRegenerationPowerActionEvent>
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicRegenerationPowerComponent, PsionicRegenerationDoAfterEvent>(OnDoAfter);
    }

    protected override void OnPowerUsed(Entity<PsionicRegenerationPowerComponent> psionic, ref PsionicRegenerationPowerActionEvent args)
    {
        var ev = new PsionicRegenerationDoAfterEvent(Timing.CurTime);
        var performer = args.Performer;
        var doAfterArgs = new DoAfterArgs(EntityManager, performer, psionic.Comp.UseDelay, ev, psionic);

        if (!DoAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        psionic.Comp.SaveDoAfterId(doAfterId.Value);

        Popup.PopupPredicted(Loc.GetString("psionic-regeneration-begin", ("entity", performer)),
            performer,
            performer,
            PopupType.Medium);

        _audioSystem.PlayPredicted(psionic.Comp.SoundUse, args.Performer, args.Performer, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnDoAfter(Entity<PsionicRegenerationPowerComponent> psionic, ref PsionicRegenerationDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        psionic.Comp.RemoveSavedDoAfterId();
        Dirty(psionic);

        if (!TryComp<BloodstreamComponent>(psionic, out var stream))
            return;

        // DoAfter has no way to run a callback during the process to give
        // small doses of the reagent, so we wait until either the action
        // is cancelled (by being dispelled) or complete to give the
        // appropriate dose. A timestamp delta is used to accomplish this.
        var percentageComplete = Math.Min(1f, (Timing.CurTime - args.StartedAt).TotalSeconds / psionic.Comp.UseDelay);

        var solution = new Solution();
        solution.AddReagent(psionic.Comp.ReagentId, FixedPoint2.New(psionic.Comp.EssenceAmount * percentageComplete));
        _bloodstreamSystem.TryAddToBloodstream((psionic, stream), solution);
    }
}
