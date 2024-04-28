// This file is licensed to you under the MIT license. See: https://spdx.org/licenses/MIT.html
// SPDX-FileCopyrightText: (c) 2024 pissdemon (https://github.com/pissdemon)
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Rejuvenate;
using Content.Shared.Traits;
using Content.Shared.Zombies;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Traits.Synthetic;

/// <summary>
/// Responsible for setting up many synth features, e.g. changed typing indicators or emote sounds.
/// </summary>
public abstract class SharedSynthSystem : EntitySystem
{
    // ReSharper disable InconsistentNaming
    [Dependency] protected readonly EntityManager _entityManager = default!;
    [Dependency] private readonly SharedTypingIndicatorSystem _typingIndicator = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] protected readonly SharedPointLightSystem _light = default!;
    [Dependency] protected readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    [Dependency] private readonly ILogManager _log = default!;
    protected ISawmill _sawmill = default!;

    // tuples of (energy, radius) for visor light, depending on eye/mouth coverage. overkill? yes, but it's cool
    // seeing light emission reduced when you wear glasses or a mask.
    // the mouth contributes more light than the eyes
    private readonly (float, float) _visorFullyVisibleSettings = (0.6f, 1.6f);
    private readonly (float, float) _visorEyesVisibleSettings = (0.5f, 1.4f);
    private readonly (float, float) _visorMouthVisibleSettings = (0.3f, 1.4f);


    protected EntityQuery<MobStateComponent> _mobStateQuery;
    protected EntityQuery<HumanoidAppearanceComponent> _humanoidAppearanceQuery;
    private EntityQuery<IdentityBlockerComponent> _identityBlockerQuery;
    private EntityQuery<MaskComponent> _maskQuery;
    // ReSharper restore InconsistentNaming

    // no ValidatePrototypeIdAttribute for lists yet :<
    // full-sized visors (vulps, lizards)
    private readonly ProtoId<MarkingPrototype>[] _visorMarkings =
    [
        "LizardHeadLEDVisorGlowing",
        "VulpHeadLEDVisorGlowing",
    ];

    // half-sized screens (humans, felinids, oni)
    private readonly ProtoId<MarkingPrototype>[] _screenMarkings =
    [
        "SyntheticScreenGlowing"
    ];

    [ValidatePrototypeId<TraitPrototype>]
    public static readonly ProtoId<TraitPrototype> SyntheticTrait = "Synthetic";

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _log.GetSawmill("system.synth");

        _mobStateQuery = _entityManager.GetEntityQuery<MobStateComponent>();
        _humanoidAppearanceQuery = _entityManager.GetEntityQuery<HumanoidAppearanceComponent>();
        _identityBlockerQuery = _entityManager.GetEntityQuery<IdentityBlockerComponent>();
        _maskQuery = _entityManager.GetEntityQuery<MaskComponent>();

        SubscribeLocalEvent<SynthComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SynthComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SynthComponent, DidEquipEvent>(OnDidEquip);
        SubscribeLocalEvent<SynthComponent, DidUnequipEvent>(OnDidUnequip);
        SubscribeLocalEvent<SynthComponent, WearerMaskToggledEvent>(OnWearerMaskToggled);
        SubscribeLocalEvent<SynthComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<EntityZombifiedEvent>(OnEntityZombified);
    }

    /// <summary>
    /// Handles updating the visor when an entity is zombified, since zombification makes their eyes red.
    /// Does robotic synths getting zombified make sense? NOPE. BUT WHO CARES.
    /// </summary>
    private void OnEntityZombified(ref EntityZombifiedEvent ev)
    {
        if (!TryComp<SynthComponent>(ev.Target, out var synthComponent))
            return;

        UpdateVisorLightState(ev.Target, synthComponent);
    }

    /// <summary>
    /// Triggered if admemes rejuvenate someone.
    /// </summary>
    private void OnRejuvenate(EntityUid uid, SynthComponent component, RejuvenateEvent args)
    {
        UpdateVisorLightState(uid, component);
    }

    /// <inheritdoc cref="OnDidEquip"/>
    private void OnWearerMaskToggled(EntityUid uid, SynthComponent component, WearerMaskToggledEvent args)
    {
        UpdateVisorLightState(uid, component);
    }

    /// <inheritdoc cref="OnDidEquip"/>
    private void OnDidUnequip(EntityUid uid, SynthComponent component, DidUnequipEvent args)
    {
        UpdateVisorLightState(uid, component);
    }

    /// <summary>
    /// Checks if the visor is blocked/unblocked to update the light.
    /// </summary>
    private void OnDidEquip(EntityUid uid, SynthComponent component, DidEquipEvent args)
    {
        UpdateVisorLightState(uid, component);
    }

    /// <summary>
    /// Responsible for handling the synth's visor light being on when the synth is alive, and off when dead.
    /// </summary>
    protected abstract void OnMobStateChanged(EntityUid uid, SynthComponent component, MobStateChangedEvent args);

    /// <summary>
    /// Called by events to change or validate the state of the synth's visor light.
    /// This method checks if something is blocking the visor and adjusts the light's intensity accordingly.
    /// </summary>
    /// <remarks>Gets called server-only too so make sure things are replicated to the client.</remarks>
    protected void UpdateVisorLightState(EntityUid uid, SynthComponent? component)
    {
        if (!Resolve(uid, ref component)
            || component.VisorUid is null // no visor, no light
            || !_mobStateQuery.TryComp(uid, out var mobState))
            return;

        var visorUid = component.VisorUid.Value;

        // if the synth is dead, force off
        if (mobState.CurrentState == MobState.Dead)
        {
            _light.SetEnabled(visorUid, false);
            return;
        }

        // are they wearing anything that would block their eyes or mouth? this is currently tracked by the
        // identity blocking system.
        if (!_inventory.TryGetContainerSlotEnumerator(uid, out var inventory))
        {
            _sawmill.Error($"Couldn't get container slot enumerator for {uid}");
            return;
        }

        // determine coverage
        var areEyesCovered = false;
        // right now the only non-visor thing is the screen which only has glowing eyes.
        // the regular visors on the other hand have glowing mouths too, the resolution of sprites is just too low
        // to actually represent those well.
        var isMouthCovered = component.EyeGlowOnly;

        while (inventory.NextItem(out var equippedUid, out var slot))
        {
            // make sure that it's actually something that can conceivably block the head, otherwise sticking a gas mask
            // into your pocket is gonna block your sight. this probably needs updating if there ever is something you
            // can wear that blocks your sight in another slot
            if (0 == (slot.SlotFlags & (SlotFlags.EYES | SlotFlags.HEAD | SlotFlags.EARS | SlotFlags.MASK | SlotFlags.NECK)))
                continue;

            if (!_identityBlockerQuery.TryComp(equippedUid, out var blockerComponent))
                continue;

            // many masks can be toggled, and if they are down they shouldn't block light.
            // unfortunately they still block identity when they are pulled down, which is presumably an upstream bug,
            // hence why we need to check this and can't rely on identityblockercomponent
            var isAMaskThatIsPulledDown = (slot.SlotFlags & SlotFlags.MASK) != 0 // is it a mask?
                                          && _maskQuery.TryComp(equippedUid, out var maskComponent)
                                          && maskComponent.IsToggled; // is it down?

            // for everything that is NOT a pulled down mask, set the covered flags if their section of the face is covered.
            if (!isAMaskThatIsPulledDown)
            {
                // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                areEyesCovered |= (blockerComponent.Coverage & IdentityBlockerCoverage.EYES) != 0;
                isMouthCovered |= (blockerComponent.Coverage & IdentityBlockerCoverage.MOUTH) != 0;
                // ReSharper enable BitwiseOperatorOnEnumWithoutFlags
            }

            if (areEyesCovered && isMouthCovered)
            {
                // no need to continue if they both are blocked. turn it off
                _light.SetEnabled(visorUid, false);
                return;
            }
        }

        // determine energy and radius of visor light depending on coverage
        var (energy, radius) = (areEyesCovered, isMouthCovered) switch
        {
            (false, false) => _visorFullyVisibleSettings,
            (true, false) => _visorEyesVisibleSettings,
            (false, true) => _visorMouthVisibleSettings,
            _ => throw new Exception("unreachable") // ruled out above. need a default arm still to avoid cs8509
        };

        // now set them
        // TODO: maybe don't if they didn't change, i should cache these to avoid a dirty call.
        _light.SetEnergy(visorUid, energy);
        _light.SetRadius(visorUid, radius);
        _light.SetEnabled(visorUid, true);
        if (_humanoidAppearanceQuery.TryComp(uid, out var appearance))
        {
            RaiseLocalEvent(uid, new SynthUpdateEyeColorEvent(appearance.EyeColor));
            _light.SetColor(visorUid, appearance.EyeColor);
        }
    }

    protected void OnInit(EntityUid uid, SynthComponent component, MapInitEvent args)
    {
        if (!_humanoidAppearanceQuery.HasComp(uid))
        {
            _sawmill.Error($"Can't turn {uid} into a synth because they are not humanoid!");
            return;
        }

        _sawmill.Debug($"Turning {uid} into a synthetic");

        _humanoidAppearance.SetSynthetic(uid, true);
        _typingIndicator.SetUseSyntheticVariant(uid, true);

        RaiseLocalEvent(uid, new TurnedSyntheticEvent());
    }

    /// <summary>
    /// Returns whether this appearance has a glowy visor marking. Out param contains the kind of visor it has.
    /// </summary>
    protected bool TryGetGlowyMarking(HumanoidAppearanceComponent appearance, out SynthVisorKind kind)
    {
        kind = SynthVisorKind.None;

        if (!appearance.MarkingSet.Markings.TryGetValue(MarkingCategories.Head, out var headMarkings))
            return false;

        // linq would read nicer but would allocate
        foreach (var marking in headMarkings)
        {
            if (_visorMarkings.Contains(marking.MarkingId))
            {
                kind = SynthVisorKind.Visor;
                return true;
            }
            if (_screenMarkings.Contains(marking.MarkingId))
            {
                kind = SynthVisorKind.Screen;
                return true;
            }
        }

        return false;
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
}

/// <summary>
/// Raised when a synth's visor eye color needs to be updated.
/// </summary>
public sealed class SynthUpdateEyeColorEvent(Color color) : EntityEventArgs
{
    public Color Color { get; set; } = color;
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
public enum SynthBrainVisuals : byte
{
    [UsedImplicitly]
    ContainsMind
}

/// <summary>
/// Visualizer enum for synth eye effects.
/// </summary>
[Serializable, NetSerializable]
public enum SynthVisorVisuals : byte
{
    EyeColor,
    Alive
}

/// <summary>
/// What kind of visor marking someone has.
/// </summary>
public enum SynthVisorKind
{
    /// <summary>
    /// No visor or screen.
    /// </summary>
    None,
    /// <summary>
    /// Full-sized visor, glows from eyes and sides.
    /// </summary>
    Visor,
    /// <summary>
    /// Screen, only glows from eyes.
    /// </summary>
    Screen
}
