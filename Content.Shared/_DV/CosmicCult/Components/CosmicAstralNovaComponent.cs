using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component to call back to the cosmic cult ability system regarding a collision
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CosmicAstralNovaComponent : Component
{
    [DataField]
    public DamageSpecifier CosmicNovaDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2> {
            { "Asphyxiation", 13 },
        },
    };

    [DataField]
    public bool DoStun = true;
}
