using Content.Server._DV.StationEvents.GameRules;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericFryRule))]
public sealed partial class NoosphericFryRuleComponent : Component
{
    /// <summary>
    /// The damage dealt to everyone wearing insulative gear.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            {"Heat", 10},
            {"Shock", 10},
        }
    };

    [DataField]
    public int FireStacks = 2;
}
