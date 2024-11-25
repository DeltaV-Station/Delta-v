using Content.Shared.Damage.Prototypes;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Overlays;

/// <summary>
/// This component allows you to see health bars above damageable mobs.
/// </summary>
<<<<<<< HEAD
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
=======
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Shitmed Change
>>>>>>> a3b45e4bd6 (Shitmed Update 2 - bottom text (#956))
public sealed partial class ShowHealthBarsComponent : Component
{
    /// <summary>
    /// Displays health bars of the damage containers.
    /// </summary>
<<<<<<< HEAD
    [DataField]
    [AutoNetworkedField]
=======
    [DataField, AutoNetworkedField] // Shitmed Change
>>>>>>> a3b45e4bd6 (Shitmed Update 2 - bottom text (#956))
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new()
    {
        "Biological"
    };

    [DataField]
    public ProtoId<HealthIconPrototype>? HealthStatusIcon = "HealthIconFine";
}
