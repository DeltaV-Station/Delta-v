﻿using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

public sealed partial class PainSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    [Dependency] private readonly SharedAudioSystem _IHaveNoMouthAndIMustScream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    [Dependency] private readonly MobStateSystem _mobState = default!;

    [Dependency] private readonly StandingStateSystem _standing = default!;

    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    private bool _screamsEnabled = false;
    private float _screamChance = 0.20f;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NerveComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<NerveComponent, ComponentGetState>(OnComponentGet);

        SubscribeLocalEvent<NerveComponent, BodyPartAddedEvent>(OnBodyPartAdded);
        SubscribeLocalEvent<NerveComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);

        SubscribeLocalEvent<NerveSystemComponent, MobStateChangedEvent>(OnMobStateChanged);

        _screamsEnabled = _cfg.GetCVar(SurgeryCVars.PainScreams);
        _screamChance = _cfg.GetCVar(SurgeryCVars.PainScreamChance);

        InitAffliction();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _painJobQueue.Process();

        if (!_timing.IsFirstTimePredicted)
            return;

        using var query = EntityQueryEnumerator<NerveSystemComponent>();
        while (query.MoveNext(out var ent, out var nerveSystem))
        {
            if (TerminatingOrDeleted(ent))
                continue;

            _painJobQueue.EnqueueJob(new PainTimerJob(this, (ent, nerveSystem), PainJobTime));
        }
    }

    private void OnComponentHandleState(EntityUid uid, NerveComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not NerveComponentState state)
            return;

        component.ParentedNerveSystem = GetEntity(state.ParentedNerveSystem);
        component.PainMultiplier = state.PainMultiplier;

        component.PainFeelingModifiers.Clear();
        foreach (var ((modEntity, id), modifier) in state.PainFeelingModifiers)
        {
            component.PainFeelingModifiers.Add((GetEntity(modEntity), id), modifier);
        }
    }

    private void OnComponentGet(EntityUid uid, NerveComponent comp, ref ComponentGetState args)
    {
        var state = new NerveComponentState();

        if (!TerminatingOrDeleted(comp.ParentedNerveSystem))
            state.ParentedNerveSystem = GetNetEntity(comp.ParentedNerveSystem);
        state.PainMultiplier = comp.PainMultiplier;

        foreach (var ((modEntity, id), modifier) in comp.PainFeelingModifiers)
        {
            if (!TerminatingOrDeleted(modEntity))
                state.PainFeelingModifiers.Add((GetNetEntity(modEntity), id), modifier);
        }

        args.State = state;
    }

    private void OnBodyPartAdded(EntityUid uid, NerveComponent nerve, ref BodyPartAddedEvent args)
    {
        var bodyPart = Comp<BodyPartComponent>(uid);
        if (!bodyPart.Body.HasValue)
            return;

        if (!_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var brainUid) || TerminatingOrDeleted(brainUid.Value))
            return;

        UpdateNerveSystemNerves(brainUid.Value, bodyPart.Body.Value, Comp<NerveSystemComponent>(brainUid.Value));
    }

    private void OnBodyPartRemoved(EntityUid uid, NerveComponent nerve, ref BodyPartRemovedEvent args)
    {
        var bodyPart = Comp<BodyPartComponent>(uid);
        if (!bodyPart.Body.HasValue)
            return;

        if (!_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var brainUid) || TerminatingOrDeleted(brainUid.Value))
            return;

        foreach (var modifier in brainUid.Value.Comp.Modifiers
                     .Where(modifier => modifier.Key.Item1 == uid))
            brainUid.Value.Comp.Modifiers.Remove((modifier.Key.Item1, modifier.Key.Item2));

        UpdateNerveSystemNerves(brainUid.Value, bodyPart.Body.Value, Comp<NerveSystemComponent>(brainUid.Value));
    }

    private void OnMobStateChanged(EntityUid uid, NerveSystemComponent nerveSys, MobStateChangedEvent args)
    {
        switch (args.NewMobState)
        {
            case MobState.Critical:
                var sex = Sex.Unsexed;
                if (TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoid))
                    sex = humanoid.Sex;

                PlayPainSoundWithCleanup(args.Target, nerveSys, nerveSys.CritWhimpers[sex], AudioParams.Default.WithVolume(-12f));
                nerveSys.NextCritScream = _timing.CurTime + _random.Next(nerveSys.CritScreamsIntervalMin, nerveSys.CritScreamsIntervalMax);
                break;

            case MobState.Dead:
                CleanupSounds(nerveSys);
                break;
        }
    }

    private void UpdateNerveSystemNerves(EntityUid uid, EntityUid body, NerveSystemComponent component)
    {
        component.Nerves.Clear();
        foreach (var bodyPart in _body.GetBodyChildren(body))
        {
            if (!TryComp<NerveComponent>(bodyPart.Id, out var nerve))
                continue;

            component.Nerves.Add(bodyPart.Id, nerve);
            Dirty(uid, component);

            nerve.ParentedNerveSystem = uid;
            Dirty(bodyPart.Id, nerve); // ヾ(≧▽≦*)o
        }
    }
}
