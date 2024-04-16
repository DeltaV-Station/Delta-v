// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Traits.Synthetic;

/// <summary>
/// Responsible for setting up many synth features, e.g. changed typing indicators or emote sounds.
/// </summary>
public abstract class SharedSynthSystem : EntitySystem
{
    [Dependency] private readonly SharedTypingIndicatorSystem _typingIndicator = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly ILogManager _log = default!;
    protected ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _log.GetSawmill("system.synth");
        SubscribeLocalEvent<SynthComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, SynthComponent component, MapInitEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            _sawmill.Error($"Can't turn {uid} into a synth because they are not humanoid!");
            return;
        }

        _sawmill.Debug($"Turning {uid} into a synthetic");

        _humanoidAppearance.SetSynthetic(uid, true);
        _typingIndicator.SetUseSyntheticVariant(uid, true);

        RaiseLocalEvent(uid, new TurnedSyntheticEvent()
        {
            Species = appearance.Species
        });
    }

    /// <summary>
    /// Ensures given entity is made synthetic. If a source is given, copies over settings.
    /// </summary>
    public void EnsureSynthetic(EntityUid uid, SynthComponent? source = null)
    {
        var comp = EnsureComp<SynthComponent>(uid);

        if (source is null)
            return;

        comp.AlertChance = source.AlertChance;
    }
}

/// <summary>
/// Raised when an entity has been turned synthetic.
/// </summary>
public sealed class TurnedSyntheticEvent : EntityEventArgs
{
    /// <summary>
    /// What their underlying species is.
    /// </summary>
    public ProtoId<SpeciesPrototype>? Species { get; set; }
}

/// <summary>
/// Raised when a synth has gotten hit by an EMP pulse.
/// </summary>
[Serializable, NetSerializable]
public sealed class SynthGotEmpedEvent : EntityEventArgs
{
}

/// <summary>
/// Visuals enum for synth brain lights.
/// </summary>
[Serializable, NetSerializable]
public enum SynthBrainVisuals
{
    [UsedImplicitly]
    ContainsMind
}
