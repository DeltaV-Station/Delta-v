using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

/// <summary>
/// Entities with this component are psionically insulated from a source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public abstract partial class BasePsionicPowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public virtual EntProtoId ActionProtoId { get; set; }

    /// <summary>
    /// The Loc string for the name of the power.
    /// </summary>
    [DataField, AutoNetworkedField]
    public virtual string PowerName { get; set; }

    /// <summary>
    /// The minimum glimmer amount that will be changed upon use of the psionic power.
    /// Should be lower than <see cref="MaxGlimmerChanged"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public virtual int MinGlimmerChanged { get; set; }

    /// <summary>
    /// The maximum glimmer amount that will be changed upon use of the psionic power.
    /// Should be higher than <see cref="MinGlimmerChanged"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public virtual int MaxGlimmerChanged { get; set; }
}
