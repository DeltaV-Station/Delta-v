using Content.Server._DV.Psionics.Systems;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Chat.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.GameRules;

/// <summary>
/// Forces a mind swap on a small amount of non-insulated psionic entities.
/// </summary>
internal sealed class MinorMassMindSwapRule : StationEventSystem<MinorMassMindSwapRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedMindSwapPowerSystem _mindSwap = default!;
    [Dependency] private readonly MobStateSystem _mobstateSystem = default!;
    [Dependency] private readonly PsionicSystem _psionic = default!;

    private TimeSpan _warningSoundLength;
    private ResolvedSoundSpecifier _resolvedWarningSound = string.Empty;

    protected override void Started(EntityUid uid, MinorMassMindSwapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _resolvedWarningSound = _audio.ResolveSound(component.SwapWarningSound);
        _warningSoundLength = _audio.GetAudioLength(_resolvedWarningSound);

        component.SwapTime = Timing.CurTime + component.Delay;
        component.SoundTime = component.SwapTime - _warningSoundLength;

        component.MaxNumberOfPairs = component.MaxNumberOfPairs < 1 ? 1 : component.MaxNumberOfPairs;

        var announcement = Loc.GetString("minor-mass-mind-swap-event-announcement", ("time", component.Delay.TotalSeconds));
        var sender = Loc.GetString("minor-mass-mind-swap-event-sender");

        _chat.DispatchGlobalAnnouncement(announcement, sender, true, component.AnnouncementSound, Color.White);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MinorMassMindSwapRuleComponent, GameRuleComponent>();

        while (query.MoveNext(out var uid, out var comp, out var ruleComp))
        {
            if (comp.SwapTime == null)
                continue;

            if (comp.SoundTime != null && comp.SoundTime <= Timing.CurTime)
            {
                _audio.PlayGlobal(_resolvedWarningSound, Filter.Broadcast(), true);
                comp.SoundTime = null;
                continue;
            }

            if (comp.SwapTime > Timing.CurTime)
                continue;

            SwapMinds(comp);
            comp.SwapTime = null;
            GameTicker.EndGameRule(uid, ruleComp);
        }
    }

    private void SwapMinds(MinorMassMindSwapRuleComponent component)
    {
        List<EntityUid> psionicActors = [];

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out var mobState))
        {
            if (_mobstateSystem.IsAlive(psion, mobState) && HasComp<ActorComponent>(psion) && _psionic.CanBeTargeted(psion))
                // Only a list of Players
                psionicActors.Add(psion);
        }

        // We go with 4 pairs for now
        List<EntityUid> actorsToSwap = [];
        var swapPairCount = _random.Next(1, component.MaxNumberOfPairs);

        for (; swapPairCount > 0 && psionicActors.Count > 1; swapPairCount--)
        {
            var target01 = _random.PickAndTake(psionicActors);
            var target02 = _random.PickAndTake(psionicActors);

            _mindSwap.SwapMinds(target01, target02, false, component.IsTemporary);
        }
    }
}
