using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body;

/// <summary>
/// Component that allows a body to deal or receive modified damage amounts based on their light level.
/// Requires <see cref="LightLevelHealthComponent"/> to function.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightLevelDamageMultComponent : Component
{
    [DataField]
    public float DarkReceivedMultiplier = 1.0f;
    [DataField]
    public float LightReceivedMultiplier = 1.0f;
    [DataField]
    public float LightDealtMultiplier = 1.0f;
    [DataField]
    public float DarkDealtMultiplier = 1.0f;

    [DataField]
    public DamageModifierSet? DarkReceivedModifiers = default!;
    [DataField]
    public DamageModifierSet? LightReceivedModifiers = default!;
    [DataField]
    public DamageModifierSet? DarkDealtModifiers = default!;
    [DataField]
    public DamageModifierSet? LightDealtModifiers = default!;
}
