// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DeltaV.Traits.Synthetic;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Traits.Synthetic;

public sealed class SynthSystem : SharedSynthSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    [ValidatePrototypeId<ReagentPrototype>]
    private readonly ProtoId<ReagentPrototype> _reagentSynthBloodId = "SynthBlood";
    [ValidatePrototypeId<SpeciesPrototype>]
    private readonly ProtoId<SpeciesPrototype> _speciesDionaId = "diona";
    [ValidatePrototypeId<EntityPrototype>]
    private readonly ProtoId<EntityPrototype> _synthBrainId = "OrganSynthBrain";
    [ValidatePrototypeId<EmoteSoundsPrototype>]
    private readonly ProtoId<EmoteSoundsPrototype> _emoteSoundsId = "UnisexSilicon";

    private const string BrainSlot = "brain";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SynthComponent, TurnedSyntheticEvent>(OnTurnedSynthetic);
        SubscribeLocalEvent<SynthComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<SynthComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<SynthBrainComponent, MindAddedMessage>(OnBrainMindAdded);
        SubscribeLocalEvent<SynthBrainComponent, MindRemovedMessage>(OnBrainMindRemoved);
    }

    /// <summary>
    /// Handles vulnerability of synths to EMP pulses.
    /// </summary>
    private void OnEmpPulse(EntityUid uid, SynthComponent component, EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        // shit fucks you up
        _popup.PopupEntity("You feel your electronics freak out!", uid, uid, PopupType.LargeCaution);
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5), false);
        _statusEffects.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromSeconds(15), false, "StutteringAccent");

        // tell their client to show the emp effect
        RaiseNetworkEvent(new SynthGotEmpedEvent(), uid);
    }

    /// <summary>
    /// Handles updating sprite visuals for the synth brain.
    /// </summary>
    private void OnBrainMindAdded(EntityUid uid, SynthBrainComponent component, MindAddedMessage args)
    {
        _appearance.SetData(uid, SynthBrainVisuals.ContainsMind, true);
    }

    /// <inheritdoc cref="OnBrainMindAdded"/>
    private void OnBrainMindRemoved(EntityUid uid, SynthBrainComponent component, MindRemovedMessage args)
    {
        _appearance.SetData(uid, SynthBrainVisuals.ContainsMind, false);
    }

    /// <summary>
    /// Handles server-only logic for turning someone into a synth.
    /// </summary>
    private void OnTurnedSynthetic(EntityUid uid, SynthComponent component, TurnedSyntheticEvent args)
    {
        // Give them synth blood. Ion storm notif is handled in that system
        _bloodstream.ChangeBloodReagent(uid, _reagentSynthBloodId);
        // dionae turn into nymphs, so eh
        if (args.Species != _speciesDionaId)
            ReplaceBrain(uid);
    }

    /// <summary>
    /// Replaces the brain of a given entity with a synth one.
    /// </summary>
    private void ReplaceBrain(EntityUid uid)
    {
        var debrained = false;
        var brainContainingBodyPartUid = EntityUid.Invalid;

        // if (!TryComp<TransformComponent>(uid, out var transform))
        //     return; // wat

        var bodyRootPartResult = _body.GetRootPartOrNull(uid);
        if (bodyRootPartResult is null)
            return; // no root body part, wtf?
        var bodyRootPartUid = bodyRootPartResult.Value.Entity;

        // swap me some organs. try and find the brain of this person. hopefully they just have one
        foreach (var (maybeOldBrainUid, organ) in _body.GetBodyOrgans(uid))
        {
            // it better be a brain
            if (!HasComp<BrainComponent>(maybeOldBrainUid))
                continue;

            // try to find the body part that contains the brain since we should probably insert a new brain
            // into exactly the place we took it out from

            foreach (var (partUid, _) in _body.GetBodyPartChildren(bodyRootPartUid))
            {
                if (_body.GetPartOrgans(partUid).All(partOrgan => partOrgan.Id != maybeOldBrainUid))
                    continue;

                brainContainingBodyPartUid = partUid;
                break;
            }

            // found a brain but not its parent!?
            if (brainContainingBodyPartUid == EntityUid.Invalid)
                break;

            // we *have* to delete the brain component on it first before we get rid of it...
            // why? because it will transfer the mind from the body it was in when we take it out
            // if we delete it right afterwards that's kind of a problem. can't defer any of this btw
            RemComp<BrainComponent>(maybeOldBrainUid);
            _body.RemoveOrgan(maybeOldBrainUid, organ);
            Del(maybeOldBrainUid);
            debrained = true;
            break;
        }

        // if we can't replace their brain it's over
        if (!debrained || brainContainingBodyPartUid == EntityUid.Invalid)
            return;

        // new brain time
        var newBrainUid = Spawn(_synthBrainId);
        if (!_body.InsertOrgan(brainContainingBodyPartUid, newBrainUid, BrainSlot))
        {
            // nevermind
            QueueDel(newBrainUid);
            _sawmill.Error($"Couldn't insert new brain into {uid}! They are now brainless.");
        }
    }

    /// <summary>
    ///  Handles the ability for synthetics to use borg emotes.
    /// </summary>
    private void OnEmote(EntityUid uid, SynthComponent component, EmoteEvent args)
    {
        // can't beep when you're muted, silly!
        if (args.Handled || HasComp<MutedComponent>(uid))
            return;

        // missing the prototype!?
        if (!_proto.TryIndex(_emoteSoundsId, out var emoteSoundsPrototype))
            return;

        // try to play a cyborg sound
        args.Handled = _chat.TryPlayEmoteSound(uid, emoteSoundsPrototype, args.Emote);

        // TODO: we can also handle sound overrides this way
    }
}
