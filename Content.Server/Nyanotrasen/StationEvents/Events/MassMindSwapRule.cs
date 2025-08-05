using Content.Server.Abilities.Psionics;
using Content.Server.Chat.Systems;
using Content.Server.Psionics;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Nyanotrasen.StationEvents.Events;

/// <summary>
/// Forces a mind swap on all non-insulated potential psionic entities.
/// </summary>
internal sealed class MassMindSwapRule : StationEventSystem<MassMindSwapRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private float _warningSoundLength;
    private ResolvedSoundSpecifier _resolvedWarningSound = String.Empty;
    protected override void Started(EntityUid uid, MassMindSwapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        component.RemainingTime = component.Timer;

        _resolvedWarningSound = _audio.ResolveSound(component.SwapWarningSound);
        _warningSoundLength = (float) _audio.GetAudioLength(_resolvedWarningSound).TotalSeconds;

        var announcement = Loc.GetString("mass-mind-swap-event-announcement", ("time", component.Timer));
        var sender = Loc.GetString("mass-mind-swap-event-sender");
        _chat.DispatchGlobalAnnouncement(announcement, sender, true, component.AnnouncementSound, Color.White);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MassMindSwapRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var comp, out var ruleComp))
        {
            comp.RemainingTime -= frameTime;
            if (comp.RemainingTime <= _warningSoundLength && !comp.PlayedWarningSound)
            {
                _audio.PlayGlobal(_resolvedWarningSound, Filter.Broadcast(), true);
                comp.PlayedWarningSound = true;
            }
            if (comp.RemainingTime <= 0f && !comp.Started)
            {
                SwapMinds(comp);
                comp.Started = true;
                GameTicker.EndGameRule(uid, ruleComp);
            }
        }
    }

    private void SwapMinds(MassMindSwapRuleComponent component)
    {
        List<EntityUid> psionicPool = new();
        List<EntityUid> psionicActors = new();

        var query = EntityQueryEnumerator<PotentialPsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out _))
        {
            if (_mobStateSystem.IsAlive(psion) && !HasComp<PsionicInsulationComponent>(psion))
            {
                psionicPool.Add(psion);

                if (HasComp<ActorComponent>(psion))
                {
                    // This is so we don't bother mindswapping NPCs with NPCs.
                    psionicActors.Add(psion);
                }
            }
        }

        // Shuffle the list of candidates.
        _random.Shuffle(psionicPool);

        foreach (var actor in psionicActors)
        {
            do
            {
                if (psionicPool.Count == 0)
                    // We ran out of candidates. Exit early.
                    return;

                // Pop the last entry off.
                var other = psionicPool[^1];
                psionicPool.RemoveAt(psionicPool.Count - 1);

                if (other == actor)
                    // Don't be yourself. Find someone else.
                    continue;

                // A valid swap target has been found.
                // Remove this actor from the pool of swap candidates before they go.
                psionicPool.Remove(actor);

                // Do the swap.
                _mindSwap.Swap(actor, other);
                if (!component.IsTemporary)
                {
                    _mindSwap.GetTrapped(actor);
                    _mindSwap.GetTrapped(other);
                }
            } while (true);
        }
    }
}
