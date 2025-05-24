using Content.Shared._DV.Vampires.EntitySystems;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Vampires.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedVampireSystem))]
public sealed partial class VampireComponent : Component
{
    /// <summary>
    /// Set of unique entities which have been drained of their blood.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> UniqueVictims = [];

    /// <summary>
    /// The timestamp at which this vampire last drained a victim's blood.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastDrainedTime;

    /// <summary>
    /// How long blood should still visible on the vampire after draining blood.
    /// </summary>
    [DataField]
    public TimeSpan DrainVisibleDuration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether this is a progenitor vampire, or one of their lesser spawn.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsLesserVampire = false;

    /// <summary>
    /// The base amount of stamina damage the victim will take from the hypnotic gaze ability.
    /// </summary>
    [DataField]
    public float BaseHypnoticDamage = 20;

    /// <summary>
    /// The additional amount of stamina damage a victim will take, which scales with unique
    /// victims the vampire has drained.
    /// </summary>
    [DataField]
    public float BonusHypnoticDamageScale = 5;

    /// <summary>
    /// Bonus resistances for brute damage (Blunt, Slash, Pierce) per unique victim.
    /// </summary>
    [DataField]
    public float BonusResistancesPerUnique = 0.01f;

    /// <summary>
    /// Maximum bonus resists from unique victims.
    /// </summary>
    [DataField]
    public float MaximumBonusResists = 0.30f;

    /// <summary>
    /// The current bonus resistances from unique victims.
    /// </summary>
    [ViewVariables]
    public DamageModifierSet BonusResistances = new()
    {
        Coefficients = {
            { "Blunt", 1f },
            { "Slash", 1f },
            { "Pierce", 1f },
        }
    };
}
