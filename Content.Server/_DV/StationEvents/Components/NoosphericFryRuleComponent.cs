using Content.Server._DV.StationEvents.GameRules;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

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
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            {"Heat", 10},
            {"Shock", 10},
        }
    };

    [DataField]
    public int FireStacks = 2;
}
