using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.ChronicPain.Components;

/// <summary>
/// Prevents the Chronic Pain overlay and popups from showing up. Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class ChronicPainSuppressedStatusEffectComponent : Component;
